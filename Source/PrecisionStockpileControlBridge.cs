using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace SelectedItems
{
    public static class PrecisionStockpileControlBridge
    {
        private static bool initialized;
        private static Type contextType;
        private static Type dataType;
        private static Type widgetsType;
        private static Type thingDefPatchType;
        private static Type categoryPatchType;
        private static FieldInfo activeField;
        private static FieldInfo dataField;
        private static MethodInfo groupOfMethod;
        private static MethodInfo hasLimitMethod;
        private static MethodInfo getLimitMethod;
        private static MethodInfo compactGroupLimitMethod;
        private static MethodInfo fullGroupLimitMethod;
        private static MethodInfo compactLimitMethod;
        private static MethodInfo fullLimitMethod;
        private static MethodInfo drawLimitMarkerMethod;
        private static MethodInfo openRowFloatMenuMethod;
        private static MethodInfo openCategoryFloatMenuMethod;

        public static bool Loaded => TryInit();

        public static bool Active => SelectedItemsMod.Settings?.IntegratePrecisionStockpileControlRendering == true && TryInit() && activeField != null && (bool)activeField.GetValue(null);

        public static bool HasThingDefControls(ThingDef def)
        {
            if (!Active || def == null)
            {
                return false;
            }

            try
            {
                object data = dataField?.GetValue(null);
                if (data == null)
                {
                    return false;
                }

                object group = groupOfMethod?.Invoke(data, new object[] { def });
                bool hasLimit = hasLimitMethod != null && (bool)hasLimitMethod.Invoke(data, new object[] { def });
                return group != null || hasLimit;
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Selected Items] Precision Stockpile Control data check failed: " + ex, 91422103);
                return false;
            }
        }

        public static bool HasCategoryControls(ThingCategoryDef categoryDef)
        {
            if (!Active || categoryDef == null)
            {
                return false;
            }

            foreach (ThingDef def in categoryDef.DescendantThingDefs)
            {
                if (HasThingDefControls(def))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool DrawThingDefControls(Rect rowRect, Rect checkRect, ThingDef def)
        {
            if (!HasThingDefControls(def))
            {
                return false;
            }

            try
            {
                object data = dataField?.GetValue(null);
                if (data == null)
                {
                    return false;
                }

                object group = groupOfMethod?.Invoke(data, new object[] { def });
                object limit = getLimitMethod?.Invoke(data, new object[] { def });
                bool hasLimit = hasLimitMethod != null && (bool)hasLimitMethod.Invoke(data, new object[] { def });
                if (group == null && !hasLimit)
                {
                    return false;
                }

                string compact = group != null
                    ? compactGroupLimitMethod?.Invoke(null, new[] { group }) as string
                    : compactLimitMethod?.Invoke(null, new[] { limit, def }) as string;
                string full = group != null
                    ? fullGroupLimitMethod?.Invoke(null, new[] { group }) as string
                    : fullLimitMethod?.Invoke(null, new[] { limit, def }) as string;

                DrawLimit(checkRect, compact, full);
                drawLimitMarkerMethod?.Invoke(null, new object[] { checkRect });
                DrawEditMenu(rowRect, def);
                return true;
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Selected Items] Precision Stockpile Control integration failed: " + ex, 91422101);
                return false;
            }
        }

        public static void OpenThingDefContextMenu(Rect rowRect, ThingDef def)
        {
            if (!Active || def == null)
            {
                return;
            }

            try
            {
                DrawEditMenu(rowRect, def);
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Selected Items] Precision Stockpile Control context menu failed: " + ex, 91422104);
            }
        }

        public static bool DrawCategoryControls(Rect rowRect, Rect checkRect, ThingCategoryDef categoryDef)
        {
            if (!HasCategoryControls(categoryDef))
            {
                return false;
            }

            try
            {
                drawLimitMarkerMethod?.Invoke(null, new object[] { checkRect });
                if (Event.current != null && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rowRect.Contains(Event.current.mousePosition))
                {
                    openCategoryFloatMenuMethod?.Invoke(null, new object[] { categoryDef });
                    Event.current.Use();
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Selected Items] Precision Stockpile Control category integration failed: " + ex, 91422102);
                return false;
            }
        }

        public static void OpenCategoryContextMenu(Rect rowRect, ThingCategoryDef categoryDef)
        {
            if (!Active || categoryDef == null)
            {
                return;
            }

            try
            {
                if (Event.current != null && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rowRect.Contains(Event.current.mousePosition))
                {
                    openCategoryFloatMenuMethod?.Invoke(null, new object[] { categoryDef });
                    Event.current.Use();
                }
            }
            catch (Exception ex)
            {
                Log.ErrorOnce("[Selected Items] Precision Stockpile Control category context menu failed: " + ex, 91422105);
            }
        }

        private static void DrawLimit(Rect checkRect, string compact, string full)
        {
            if (string.IsNullOrEmpty(compact))
            {
                return;
            }

            Rect labelRect = new Rect(checkRect.x - 88f, checkRect.y + 2f, 82f, 20f);
            GameFont font = Text.Font;
            TextAnchor anchor = Text.Anchor;
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.MiddleRight;
            GUI.color = new Color(0.72f, 0.82f, 1f);
            Widgets.Label(labelRect, compact);
            GUI.color = Color.white;
            Text.Anchor = anchor;
            Text.Font = font;
            if (!string.IsNullOrEmpty(full))
            {
                TooltipHandler.TipRegion(labelRect, full);
            }
        }

        private static void DrawEditMenu(Rect rowRect, ThingDef def)
        {
            if (Event.current != null && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rowRect.Contains(Event.current.mousePosition))
            {
                openRowFloatMenuMethod?.Invoke(null, new object[] { def });
                Event.current.Use();
            }
        }

        private static bool TryInit()
        {
            if (initialized)
            {
                return contextType != null;
            }

            initialized = true;
            Assembly assembly = null;
            foreach (Assembly candidate in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (candidate.GetName().Name == "PrecisionStockpileControl")
                {
                    assembly = candidate;
                    break;
                }
            }
            if (assembly == null)
            {
                return false;
            }

            contextType = assembly.GetType("PrecisionStockpileControl.PscUiContext");
            dataType = assembly.GetType("PrecisionStockpileControl.PscStorageData");
            widgetsType = assembly.GetType("PrecisionStockpileControl.PscUiWidgets");
            thingDefPatchType = assembly.GetType("PrecisionStockpileControl.Listing_TreeThingFilter_DoThingDef_Patch");
            categoryPatchType = assembly.GetType("PrecisionStockpileControl.Listing_TreeThingFilter_DoCategory_Patch");

            activeField = contextType?.GetField("Active", BindingFlags.Public | BindingFlags.Static);
            dataField = contextType?.GetField("Data", BindingFlags.Public | BindingFlags.Static);
            groupOfMethod = dataType?.GetMethod("GroupOf", BindingFlags.Public | BindingFlags.Instance);
            hasLimitMethod = dataType?.GetMethod("HasLimit", BindingFlags.Public | BindingFlags.Instance);
            getLimitMethod = dataType?.GetMethod("GetLimit", BindingFlags.Public | BindingFlags.Instance);
            compactGroupLimitMethod = widgetsType?.GetMethod("CompactGroupLimit", BindingFlags.Public | BindingFlags.Static);
            fullGroupLimitMethod = widgetsType?.GetMethod("FullGroupLimit", BindingFlags.Public | BindingFlags.Static);
            compactLimitMethod = widgetsType?.GetMethod("CompactLimit", BindingFlags.Public | BindingFlags.Static, null, new[] { assembly.GetType("PrecisionStockpileControl.PscDefLimit"), typeof(ThingDef) }, null);
            fullLimitMethod = widgetsType?.GetMethod("FullLimit", BindingFlags.Public | BindingFlags.Static, null, new[] { assembly.GetType("PrecisionStockpileControl.PscDefLimit"), typeof(ThingDef) }, null);
            drawLimitMarkerMethod = widgetsType?.GetMethod("DrawLimitMarker", BindingFlags.Public | BindingFlags.Static);
            openRowFloatMenuMethod = thingDefPatchType?.GetMethod("OpenRowFloatMenu", BindingFlags.NonPublic | BindingFlags.Static);
            openCategoryFloatMenuMethod = categoryPatchType?.GetMethod("OpenCategoryFloatMenu", BindingFlags.NonPublic | BindingFlags.Static);

            return contextType != null;
        }
    }
}
