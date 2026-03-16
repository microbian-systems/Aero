# Analysis of Aero.Cms* projects

All projects starting with Aero.Cms* have just been brought into the solution. Their purpose is to provide a CMS solution. It has the ability to run multpile sites in one deployment, manage users, roles, permissions, pages, posts, media, aliases, etc.

Goal: We are creating a new blazor based manager for the Aero platform. The old manager is written in asp.net core and Vuejs. We want to remove the Vuejs dependency and use blazor instead.

The Vuejs app is located here: src\Aero.Cms.Manager\assets\src

Checkout the [aero_cms_overview](./aero_cms_overview.md) for an overview of the current block system and existing manager.

Get familiar with the older documentation here: docs/
    - You need to understand the old manager and how it works and its implementation details.  Those details can be found in the docs folder here: docs/05. Manager Architecture

Realize we have moved from using EFCore to Marten as our ORM (this port has already been done). We only support Postgres (via MartenDB). This means the (new) relationships were modeled with nosql modeling startegies.  To get familiar with the current datashapes look at the code here: src\Aero.Cms.Data and here src\Aero.Cms\Models   -  We need the new manager to work with this new model paradigm

Analyze the following c# projects:
    - Aero.Cms
    - Aero.Cms.AspNetCore
    - Aero.Cms.AspNetCore.Hosting
    - Aero.Cms.AspNetCore.Identity
    - Aero.Cms.AttributeBuilder
    - Aero.Cms.Azure.BlobStorage
    - Aero.Cms.Data
    - Aero.Cms.ImageSharp
    - Aero.Cms.Local.FileStorage
    - Aero.Cms.Manager
    - Aero.Cms.Manager.LocalAuth
    - Aero.Cms.Manager.Localization
    - Aero.Cms.Manager.TinyMCE
    - Aero.Cms.WebApi

We are going to replace the Aero.Cms.Manager with a new .net blazor based manager. Do not delete the old manager, but make sure it is not used in the new manager.

The projects for the new blazor UI (a blazor maui hybrid app with web, mobile and desktop apps) exist here: 

Aero.Cms.Manager.UI
Aero.Cms.Manager.UI.AppHost
Aero.Cms.Manager.UI.ServiceDefaults
Aero.Cms.Manager.UI.Shared
Aero.Cms.Manager.UI.Web
Aero.Cms.Manager.UI.WebClient

We want to use a components architecture for each UI compomenent (i.e. - pages, posts, site, alias, editors, media, etc.) as recommended by microsoft. you can use the microsoft mcp server for references on this. The shared components folder lives here: src\Aero.Cms.Manager.UI.Shared


## Feature Map 

The page editor will be the most complicated.  It should be able to add and edit page blocks like the original. In fact, we should pull in the Vuejs code and reuse its funcitonality (for the page/post editor only)

### CMS Features

Dashboard:
    - Site selector
    - Site stats
    - Quick links
    - Recent activity
    - Recent Errors

Site Management:
    - Sites
        - Multi-tenancy (each site has its own pages, posts, media, aliases, etc.)
        - Analyze existing legacy manager code to see how its implemented
    - Domains/Hostnames
    - Aliases

Content (analyze existing implementation):
    - NavBars
        - Menu Items
            - drop down
            - search
            - icon
    - Pages
        - Page List
        - Page Editor
            - Markdown Editor
            - HTML Editor
            - Localization: language translation service (call to an AI agent - mock for now)
    - Posts (blog)
        - Post Series (default but can have more - has a specific route based on series name, i.e. - /blog/{series-name}/{post-slug})
            - Post List
        - Post Editor (blog page/detail)
        - Markdown Post Editor
            - Markdown Editor
            - HTML Editor
            - Localization: language translation service (call to an AI agent - mock for now)
        - Blog Importer
    - Content Blocks (looks like these are built-in here: src\Aero.Cms\Extend\Block.cs)

Media
    - Upload
    - Images + Videos + Audio + Files
    - Folders
    - Storage Providers

Users
    - Users
    - Roles
    - Permissions

Localization
    - Allowed Languages
        - Defaults:
            - English - en-US (default)
            - English - en-GB
            - English - en-AU
            - English - en-CA
            - Spanish - es-ES
            - Spanish - es-MX
            - French - fr-FR
            - German - de-DE
            - Italian - it-IT
            - Portuguese - pt-PT
            - Portuguese - pt-BR
            - Russian - ru-RU
            - Ukrainian - uk-UA
            - Chinese - zh-CN
            - Japanese - ja-JP
            - Korean - ko-KR
            - Hindi - hi-IN

System
    - Settings / configuration
    - Modules
        - Import


----------

## Implementation Plan

    # Analyze the existing Aero.Cms.Manager project and understand its architecture and implementation details
    # Analyze the existing Aero.Cms.Manager.UI projects and understand their architecture and implementation details
    # The navigation URL for the new manager should be /admin
        - do NOT overwrite existing manager URL or functionality
    # Build the UI blazor components first (no backend integration yet)
        - Tailwind for css
        - Alpine.js for client-side interactivity (libman, no npm)
    # Integrate the UI components with the backend services
    # Make sure to seed any new data (i.e. - allowed languages, etc.) at startup (there is a Seed class already)
    # Build the Desktop MAUI app
    # Build the mobile MAUI apps (android and ios)
    # No NPM - use libman for all client-side dependencies