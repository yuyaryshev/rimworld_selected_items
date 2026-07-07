# Selected Items

RimWorld 1.6 mod that improves the standard Storage tab.

When a stockpile, shelf, or other storage container allows only a small number of
specific items, the mod shows a compact selected-items list above the storage
filter tree. Each row keeps a checkbox, so the item can be allowed or disallowed
directly from the summary while the underlying tree stays synchronized.

The list is shown only when the storage filter starts with a small enough number
of selected concrete item defs. The threshold is configurable in the mod settings
and applies immediately without restarting RimWorld. Newly selected items are
kept in the summary up to twice the threshold. A separate setting controls when
the summary starts using its own scrollbar.

## Build

```powershell
dotnet build .\Source\SelectedItems.csproj -c Release
```

The build output is written to `1.6\Assemblies\SelectedItems.dll`.
