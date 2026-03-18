# Aero.Cms Block System Overview

This document provides a detailed overview of how blocks and fields work in the `Aero.Cms` project and how they are rendered in the editor.

## 1. Block Architecture

Blocks in Aero.Cms are designed using a **metadata-driven approach** that combines C# classes with specialized attributes.

### Core Components
- **`Block` (Base Class)**: The abstract root for all blocks. It inherits from `Entity` and provides basic properties like `Type`.
- **`BlockTypeAttribute`**: Used to decorate block implementations. It defines:
  - `Name` & `Category`: Used for categorization in the block selector.
  - `Icon`: FontAwesome icon for the UI.
  - `Component`: **Critical property** that specifies the string ID of the UI component used to render the block in the editor.
  - `Width`: Controls the editor layout (Centered, Wide, Full).
- **`BlockGroup`**: A specialized block that can contain a collection of other blocks, enabling nested structures (e.g., columns or galleries).

### Example Block: `VideoBlock`
```csharp
[BlockType(Name = "Video", Category = "Media", Icon = "fas fa-video", Component = "video-block")]
public class VideoBlock : Block
{
    public VideoField Body { get; set; }

    public override string GetTitle() => Body?.Media?.Filename ?? "No video selected";
}
```

---

## 2. Field Architecture

Fields represent the data points within a block (or a page).

- **`Field` (Base Class)**: The root for all field types.
- **`FieldTypeAttribute`**: Similar to blocks, this defines the UI `Component` (e.g., `video-field`, `text-field`) used in the manager.
- **`IField` Interface**: Ensures consistency across different field implementations.

---

## 3. Registration and Discovery

The `App` singleton (`src\Aero.Cms\App.cs`) serves as the central registry for the CMS.

1.  **Manual Registration**: In the `App` static constructor, blocks and fields are registered:
    ```csharp
    Instance._blocks.Register<Extend.Blocks.VideoBlock>();
    Instance._fields.Register<Extend.Fields.VideoField>();
    ```
2.  **Reflection-Based Setup**: `AppBlockList.OnRegister` inspects the class via reflection to:
    - Extract `BlockTypeAttribute` values.
    - Automatically register any internal fields found on the block properties.
    - Map `Init` and `InitManager` methods for custom initialization logic.

---

## 4. How Rendering Works in the Editor

Rendering in the Aero.Cms editor is decoupled from the backend logic via a component-mapping system.

### The Rendering Pipeline
1.  **Component Mapping**: The manager UI (likely a Blazor or JS application) receives the `Component` string from the block's metadata.
2.  **Dynamic Instantiation**: The UI instantiates the client-side component (e.g., `video-block.vue` or a Blazor component named `VideoBlock`) and passes the block's data as a model.
3.  **Manager Initialization**: When the manager loads a content model, it uses `IContentFactory.InitManagerAsync()`.
    - This internal service recursively calls `InitFieldAsync` with `managerInit: true`.
    - This allows fields to load additional metadata or perform setup required only during editing (e.g., fetching asset URLs).

---

## 5. Persistence and Serialization

Data is persisted as JSON using custom serializers to handle the polymorphic nature of blocks.

- **`JsonDerivedType`**: The `Block` base class defines derived types for the JSON serializer, allowing it to store a `List<Block>` containing mixed types (`TextBlock`, `VideoBlock`, etc.).
- **Field Serializers**: Each field type has a dedicated serializer (e.g., `VideoFieldSerializer`) registered in `App.cs`. These handle the conversion between the database representation (often just IDs or simple values) and the rich C# field objects.

---

## Summary of Roles
| Component | Responsibility |
| :--- | :--- |
| **Block Class** | Defines the data structure and business logic. |
| **Attribute** | Provides UI metadata and component mapping. |
| **`App.cs`** | Orchestrates registration and global state. |
| **`ContentFactory`** | Prepared models for display or editing. |
| **Serializers** | Bridges the gap between C# objects and JSON storage. |

---

## 6. CMS Manager Architecture (`Aero.Cms.Manager`)

The management interface is built as an ASP.NET Core Razor Pages application integrated with **Vue.js** for a dynamic, reactive editorial experience.

### Core Technologies
- **Backend**: Razor Pages (`Areas/Manager/Pages`) providing the shell and initial routing.
- **Frontend**: Vue.js (v2) managing the state of the content being edited.
- **Communication**: Standard `fetch` API for interaction with internal Manager REST endpoints (`/manager/api/...`).

---

## 7. Page & Post Editor Deep Dive

Both editors (`PageEdit.cshtml` and `PostEdit.cshtml`) follow a consistent pattern but differ in metadata focus.

### A. Initialization & Loading Flow
1.  **Direct Navigation**: User hits `/manager/page/edit/{id}`.
2.  **Razor Execution**: The page loads the basic HTML structure and injects core scripts:
    - `Aero.components.min.js`: Pre-compiled Vue components for blocks/fields.
    - `Aero.pageedit.min.js`: The Vue instance definition.
3.  **Bootstrapping**: An inline script calls `Aero.pageedit.load("{id}")`.
4.  **API Fetch**: The Vue method triggers a GET request to `manager/api/page/{id}`.
5.  **Model Binding**: The JSON response is mapped to the Vue `data` object via the `bind()` method, populating `title`, `blocks`, `regions`, etc.
6.  **UI Updates**: Vue's reactivity system renders the blocks and fields automatically.

### B. Manipulation Logic
Users manipulate content through several reactive actions:
- **Adding Blocks**: `Aero.blockpicker.open()` triggers a modal. Selecting a block calls `addBlock(type, pos)` which fetches a fresh JSON structure from the API and `splices` it into the `blocks` array.
- **Sorting**: Integrated with `sortable.js`. Dragging a block handle updates the DOM, which then triggers `moveBlock(from, to)` to synchronize the underlying Vue array.
- **Settings**: Sidebars (`_PageSettings.cshtml` or `_PostSettings.cshtml`) handle metadata like Slugs, SEO tokens (OpenGraph), and Permissons.

### C. Editor Specifics
| Feature | Page Editor | Post Editor |
| :--- | :--- | :--- |
| **Structure** | Hierarchical (`parentId`). | Blog-centric (`blogId`). |
| **Regions** | Supports multiple named content regions. | Simplified region support. |
| **Taxonomy** | Primary focus on site navigation. | Heavy use of Categories & Tags (Select2). |
| **Custom Editors** | Can load entire custom Vue UI for regions. | Primarily block-based. |

---

## 8. Block & Field Inventory

The editor dynamically loads components from the `assets/src/js/components` directory.

### Available Blocks
These components provide the editorial UI for the structure defined in `Aero.Cms`.
- **Media**: `audio-block`, `image-block`, `video-block`, `image-gallery-block` (nested).
- **Structure**: `block-group` (vertical/horizontal), `separator-block`, `column-block`.
- **Content**: `text-block`, `html-block`, `markdown-block`, `quote-block`.
- **References**: `content-block`, `page-block`, `post-block`.

### Available Fields
Fields are the atomic units inside blocks or regions.
- **Basic**: `string-field`, `text-field`, `html-field`, `markdown-field`, `number-field`.
- **Selection**: `checkbox-field`, `color-field`, `date-field`, `select-field`, `data-select-field`.
- **Assets**: `image-field`, `video-field`, `audio-field`, `document-field`, `media-field`.
- **CMS specific**: `page-field`, `post-field`, `content-field`, `archivepage-field`.
