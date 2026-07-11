using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace SelectedItems
{
    public static class StorageNavigation
    {
        private const int SearchRadius = 3;

        public enum Direction
        {
            Left,
            Right,
            Up,
            Down
        }

        public static bool TryFindNeighbor(Direction direction, out object neighbor)
        {
            neighbor = null;
            object current = Find.Selector?.SingleSelectedObject;
            if (!TryGetSearchContext(current, out Map map, out List<IntVec3> sourceCells))
            {
                return false;
            }

            CandidateScore bestScore = CandidateScore.Invalid;
            HashSet<object> seen = new HashSet<object>();
            foreach (IntVec3 sourceCell in sourceCells)
            {
                for (int dx = -SearchRadius; dx <= SearchRadius; dx++)
                {
                    for (int dz = -SearchRadius; dz <= SearchRadius; dz++)
                    {
                        if (dx == 0 && dz == 0)
                        {
                            continue;
                        }
                        if (!MatchesDirection(dx, dz, direction))
                        {
                            continue;
                        }

                        IntVec3 cell = new IntVec3(sourceCell.x + dx, sourceCell.y, sourceCell.z + dz);
                        if (!cell.InBounds(map))
                        {
                            continue;
                        }

                        CandidateScore score = CandidateScore.ForOffset(dx, dz, direction);
                        CheckThingCandidates(current, map, cell, seen, ref neighbor, ref bestScore, score);
                        CheckZoneCandidate(current, map, cell, seen, ref neighbor, ref bestScore, score);
                    }
                }
            }

            return neighbor != null;
        }

        public static void SelectNeighbor(Direction direction)
        {
            if (!TryFindNeighbor(direction, out object neighbor))
            {
                return;
            }

            Find.Selector.ClearSelection();
            StorageFilterSelection.ExpandNextSnapshot();
            Find.Selector.Select(neighbor);
            if (SelectedItemsMod.Settings?.OpenStorageTabOnSelect != false)
            {
                InspectPaneUtility.OpenTab(typeof(ITab_Storage));
            }
        }

        public static bool HasStorageTab(object obj)
        {
            if (obj is Thing thing)
            {
                return HasStorageTab(thing.GetInspectTabs());
            }
            if (obj is Zone zone)
            {
                return HasStorageTab(zone.GetInspectTabs());
            }
            return obj is IStoreSettingsParent parent && parent.StorageTabVisible;
        }

        private static bool HasStorageTab(IEnumerable<InspectTabBase> tabs)
        {
            if (tabs == null)
            {
                return false;
            }
            return tabs.Any(tab => tab != null && typeof(ITab_Storage).IsAssignableFrom(tab.GetType()));
        }

        private static void CheckThingCandidates(object current, Map map, IntVec3 cell, HashSet<object> seen, ref object best, ref CandidateScore bestScore, CandidateScore score)
        {
            List<Thing> things = cell.GetThingList(map);
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (ReferenceEquals(thing, current) || !seen.Add(thing) || !IsMatchingStorage(current, thing))
                {
                    continue;
                }
                if (score.IsBetterThan(bestScore))
                {
                    bestScore = score;
                    best = thing;
                }
            }
        }

        private static void CheckZoneCandidate(object current, Map map, IntVec3 cell, HashSet<object> seen, ref object best, ref CandidateScore bestScore, CandidateScore score)
        {
            Zone zone = map.zoneManager.ZoneAt(cell);
            if (zone == null || ReferenceEquals(zone, current) || !seen.Add(zone) || !IsMatchingStorage(current, zone))
            {
                return;
            }
            if (score.IsBetterThan(bestScore))
            {
                bestScore = score;
                best = zone;
            }
        }

        private static bool IsMatchingStorage(object current, object candidate)
        {
            if (!HasStorageTab(candidate))
            {
                return false;
            }
            if (current is Thing currentThing && candidate is Thing candidateThing)
            {
                return currentThing.def == candidateThing.def;
            }
            return current is Zone_Stockpile && candidate is Zone_Stockpile;
        }

        private static bool TryGetSearchContext(object current, out Map map, out List<IntVec3> cells)
        {
            map = null;
            cells = null;

            if (current is Thing thing && HasStorageTab(thing))
            {
                map = thing.MapHeld;
                cells = new List<IntVec3> { thing.PositionHeld };
                return map != null && cells[0].IsValid;
            }
            if (current is Zone_Stockpile zone && HasStorageTab(zone))
            {
                map = zone.Map;
                cells = zone.cells;
                return map != null && cells != null && cells.Count > 0;
            }
            return false;
        }

        private static bool MatchesDirection(int dx, int dz, Direction direction)
        {
            switch (direction)
            {
                case Direction.Left:
                    return dx < 0 && Math.Abs(dx) >= Math.Abs(dz);
                case Direction.Right:
                    return dx > 0 && Math.Abs(dx) >= Math.Abs(dz);
                case Direction.Up:
                    return dz > 0 && Math.Abs(dz) > Math.Abs(dx);
                case Direction.Down:
                    return dz < 0 && Math.Abs(dz) > Math.Abs(dx);
                default:
                    return false;
            }
        }

        private struct CandidateScore
        {
            public static readonly CandidateScore Invalid = new CandidateScore(int.MaxValue, int.MaxValue);

            private readonly int perpendicularDistance;
            private readonly int primaryDistance;

            private CandidateScore(int perpendicularDistance, int primaryDistance)
            {
                this.perpendicularDistance = perpendicularDistance;
                this.primaryDistance = primaryDistance;
            }

            public static CandidateScore ForOffset(int dx, int dz, Direction direction)
            {
                switch (direction)
                {
                    case Direction.Left:
                        return new CandidateScore(Math.Abs(dz), -dx);
                    case Direction.Right:
                        return new CandidateScore(Math.Abs(dz), dx);
                    case Direction.Up:
                        return new CandidateScore(Math.Abs(dx), dz);
                    case Direction.Down:
                        return new CandidateScore(Math.Abs(dx), -dz);
                    default:
                        return Invalid;
                }
            }

            public bool IsBetterThan(CandidateScore other)
            {
                if (perpendicularDistance != other.perpendicularDistance)
                {
                    return perpendicularDistance < other.perpendicularDistance;
                }
                return primaryDistance < other.primaryDistance;
            }
        }
    }
}
