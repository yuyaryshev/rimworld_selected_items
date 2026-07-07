# Selected Items

RimWorld 1.6 mod that improves the standard Storage tab.

When a stockpile, shelf, or other storage container is selected, the mod shows a
compact selected-items header above the storage filter tree. If the storage
allows only a small number of specific items, the header expands into a short
list. Each row keeps a checkbox, so the item can be allowed or disallowed
directly from the summary while the underlying tree stays synchronized.

The header always shows the selected item count. The refresh button rebuilds the
list from currently allowed items, removing rows that were unchecked in the
summary. The expand/collapse button can show the list even when the selected
count is above the normal threshold. The threshold and the scrollbar start point
are configurable in the mod settings and apply immediately without restarting
RimWorld.

## Build

```powershell
dotnet build .\Source\SelectedItems.csproj -c Release
```

The build output is written to `1.6\Assemblies\SelectedItems.dll`.
