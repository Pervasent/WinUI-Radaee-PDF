# RadaeePDF SDK Master for Windows (WinUI 3 - Beta)
<img src="https://www.radaeepdf.com/wp-content/uploads/2024/08/solo_butterly_midres.png" style="width:100px;"> 

> **DISCLAIMER (BETA)**  
> This repository contains the **WinUI 3** version of the RadaeePDF Windows demo and is currently in **BETA**.  
> The API surface, project structure, and some features are subject to change without notice.  
> Do **not** use this version in production without thoroughly testing it in your own environment.

RadaeePDF SDK is a powerful, native PDF rendering and manipulation library for Android, iOS, and Windows UI applications. Built from true native C++ code, it provides exceptional performance and a comprehensive set of features for working with PDF documents.

## About RadaeePDF

RadaeePDF SDK is designed to solve most developers' needs regard to PDF rendering and manipulation. The SDK is trusted across industries worldwide including automotive, banking, publishing, healthcare, and more.

### Key Features

- **PDF ISO32000 Compliance** - Full support for the widely-used PDF format standard
- **High Performance** - True native code compiled from C++ sources for optimal speed
- **Annotations** - Create and manage text annotations, highlights, ink annotations, and more
- **Protection & Encryption** - Full AES256 cryptography for document security
- **Text Handling** - Search, extract, and highlight text with ease
- **Form Editing** - Create, read, and write PDF form fields (AcroForms)
- **Digital Signatures** - Sign and verify PDF documents with digital certificates
- **Multiple View Modes** - Single page, continuous scroll, and more
- **Night Mode** - Built-in dark mode support for better readability

## Requirements

Before running the demo, ensure you have the following installed:

### Visual Studio Requirements
- **Visual Studio 2022** (or later) with the following workloads:
  - **WinUI desktop development** - Desktop development with WinUI 3 and C#, optionally for C++
  - **Desktop development with C++** - With C++ (v143) tools for native components
  
### Windows SDK
- **Windows 11 SDK (10.0.22621.0)** or later

### Windows Developer Mode
You must enable Developer Mode on Windows:

1. Open **Settings**
2. Go to **System** → **For developers**
3. Enable **Developer Mode**
   - This allows you to install apps from any source, including loose files

## Nuget package

> **Note on Large Library Files**
> The static library (`*.lib`) files in this repository are very large (several hundred MB each). This is because WinUI Release builds expand all inline functions during compilation, which significantly increases the instruction size and results in much larger static library files.
>
> **Important:** These `*.lib` files are only required by the dynamic library project **RDUICom**. Once RDUICom is successfully built, it outputs normal-sized dynamic library files (`*.dll` and `*.winmd`) that are used by the application project **RadaeeWinUI**. When deploying your application, you do **not** need to include the large `*.lib` files—only the compiled `*.dll` and `*.winmd` files are required.

In this case, we use **Nuget** for publishing and managing the native library files.

To successfully extract the compressed native library, 7-zip is required on your local environment. 

Beore build the project, please restore the nuget packages first.

## clone the repository:

```powershell
git clone git@github.com:RadaeePDF-Jugaad/RadaeePDF-Master-Windows-WinUI.git
cd RadaeePDF-Master-Windows-WinUI

# From now on, a normal pull will also download/update LFS files (.lib)
git pull
```

If you already cloned this repository in the past **without** correct nuget dependency added, please re-clone the repository and restore the nuget packages.

## Quick Start - Run Demo

To quickly test the RadaeePDF SDK demo:

1. **Clone the Repository**
   - Open Visual Studio
   - Click on **Clone a repository** (or File → Clone Repository)
   - Paste the repository URL:
     ```
     https://github.com/RadaeePDF-Jugaad/RadaeePDF-Master-Windows-WinUI.git
     ```
   - Click **Clone** and wait for the project to download

2. **Keep the Repository Up to Date**
   - From a terminal, inside the cloned folder:
     ```bash
     # Standard git
     git pull
     
     # Using git-lts (if installed in your environment)
     git-lts pull
     ```

3. **Open the Solution**
   - Visual Studio should automatically open the `RadaeeWinUI.sln` solution file
   - If not, navigate to the cloned folder and double-click `RadaeeWinUI.sln`

4. **Configure Build Settings**
   - In the toolbar, set the platform to **x86** (or other platform based on your system)
   - Set the configuration to **Debug** or **Release**

5. **Build library project**
   - In solution explorer, right click on project "RDUICom" and select **Build**
   - Wait for the build to complete

6. **Build and Run the Demo**
   - Press **F5** or click the **Start** button (▶) to build and run the demo
   - The WinUI 3 application will launch on your local machine

### Manual Installation

1. Download the RadaeePDF SDK library from Git Repository

## Getting Started

### Initialize the Library

Before using RadaeePDF, initialize the library with your license key:

1. Modify the 'RDGlobal.cs' file under namespace 'RadaeeWinUI.RadaeeUtil', to add your license key:
```csharp
static public bool init()
{
    load_data();
    int ret = RDUILib.RDGlobal.Active("[YOUR-LICENSE-KEY]");
    return ret == 3;
}
```

2. Add the following code to your 'App' class to initialize and activate the library:
```csharp
using RadaeeWinUI.RadaeeUtil;

sealed partial class App : Application
{
    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        //...... your code
        RDGlobal.init();
        //...... your code
    }
}
```

### Open and Display a PDF

```csharp
using RadaeeWinUI.Helpers;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly IDocumentManager _documentManager;
    private readonly INavigationService _navigationService;
    private readonly PDFViewModel _pdfViewModel;

    private PDFDoc? _currentDocument;
    private DocumentInfo? _documentInfo;
    private bool _isDocumentLoaded;

    public MainViewModel(
    IDocumentManager documentManager,
    INavigationService navigationService,
    PDFViewModel pdfViewModel)
    {
        _documentManager = documentManager;
        _navigationService = navigationService;
        _pdfViewModel = pdfViewModel;

        _pdfViewModel.CurrentPageChanged += _pdfPageChanged;

        //...... your code
    }

    private async Task LoadDocumentAsync(StorageFile file, string password = "")
    {
        _pdfViewModel.OnDocumentClosed();
        _documentManager.CloseDocument(_currentDocument);

        _currentDocument = await _documentManager.OpenDocumentAsync(file, password);

        if (_currentDocument != null)
        {
            DocumentInfo = _documentManager.GetDocumentInfo(_currentDocument);
            _navigationService.SetTotalPages(DocumentInfo?.PageCount ?? 0);
            _navigationService.GoToPage(0);
            IsDocumentLoaded = true;

            _pdfViewModel.OnDocumentLoaded(_currentDocument);

            //...... your code

        }
    }
}
```

## Common Operations

### Get Page Count

```csharp
int pageCount = m_doc.PageCount;
```

### Navigate to a Specific Page

```csharp
MainViewModel.GoToPage(5); // Go to page 5
```

### Set View Mode

```csharp
MainViewModel.ViewModel.PDFViewModel.SwitchViewMode(ViewMode.VerticalContinuous); // Vertical scroll mode
```

### Text Highlighting (Professional License)

```csharp
uint color = 0xFFFFFF00; // Yellow color
// Highlight selected text
PDFSel.SetSelMarkup(AnnotationType.Highlight); // highlight

// Underline selected text
PDFSel.SetSelMarkup(AnnotationType.Underline); // underline

// Strikeout selected text
PDFSel.SetSelMarkup(AnnotationType.Strikeout); // strikeout

// Rod squiggly selected text
PDFSel.SetSelMarkup(AnnotationType.Squiggly); // rod squiggly
```

### Add Annotations (Professional License)

```csharp
// Add a note annotation at point (x, y)
PDFPage page = m_doc.GetPage(0); // Get the first page
if(page != null) {
    page.ObjsStart();
    page.AddAnnotTextNote(x, y);
    page.Close();
}


// Remove annotation
PDFPage page = m_doc.GetPage(0); // Get the first page
if (page != null) {
    //Start object mode
    page.ObjsStart();
    PDFAnnot annot = page.GetAnnot(0);
    if (annot != null) { 
        annot.RemoveFromPage();
    }

    //Close page and release holding resource
    page.Close();
}
```

### Save Document

```csharp
// Save changes to the same file
m_doc.Save();

// Save to a new file
m_doc.SaveAs("[path_to_save]");
```

## License Levels

RadaeePDF offers different license levels with varying features:

Visit [https://www.radaeepdf.com/](https://www.radaeepdf.com/) for detailed licensing information.

## Documentation

For complete API documentation and advanced features, visit:
- [RadaeePDF Support Portal](https://support.radaeepdf.com/)

## Support

For technical support and questions:
- Email: support@radaeepdf.com
- Website: [https://www.radaeepdf.com/](https://www.radaeepdf.com/)

## License

This SDK is commercial software. Please ensure you have a valid license before using it in production applications.

---

© 2026 RadaeePDF. All rights reserved.
