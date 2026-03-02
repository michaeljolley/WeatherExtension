# SDK Binary Inspection

**When:** You need to verify whether types/methods exist in a NuGet package DLL or WinMD, especially when documentation or previous notes may be outdated.

**How:**
```python
# Search a DLL or WinMD for type names
with open(dll_path, 'rb') as f:
    data = f.read()
for term in [b'TypeName', b'MethodName']:
    idx = data.find(term)
    if idx >= 0:
        start = max(0, idx - 40)
        end = min(len(data), idx + 80)
        context = data[start:end]
        printable = ''.join(chr(b) if 32 <= b < 127 else '.' for b in context)
        print(f'{term.decode()} at {idx}: {printable}')
    else:
        print(f'{term.decode()}: NOT FOUND')
```

**NuGet package location pattern:** `D:\packages\.nuget\packages\{package-name}\{version}\`
- `lib\{tfm}\` — Toolkit DLLs (managed types, extension methods)
- `winmd\` — WinRT metadata (interfaces, virtual methods)
- `runtimes\{rid}\native\` — Native DLLs

**Key insight:** Always check BOTH the WinMD (for interfaces/contracts) and the Toolkit DLL (for implementation types like `WrappedDockItem`). A type may exist in one but not the other.

**Used for:** Verifying DockBands API availability (GetDockBands in WinMD, WrappedDockItem in Toolkit DLL) when previous team notes said it was unavailable.
