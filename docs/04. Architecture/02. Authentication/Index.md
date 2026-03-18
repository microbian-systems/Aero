# Authentication

Aero doesn't really care **how** your users gets authenticated, whether it's the end users of your application or the administrators accessing the manager interface. Instead, by default Aero uses a claims based security model to check what the current user has access to. If you wish to use an alternative -such as Role based authorization, you can provide a delegate via the overloaded `IServiceCollection.AddAeroManager()` method in your Startup.cs file:

~~~ csharp
IServiceCollection.AddAeroManager((permission, options) =>
{
    switch (permission)
    {
        //for managing visitor comments, CommentModerator role membership is required (or Administrator role)
        case Permission.Comments:
        case Permission.CommentsApprove:
        case Permission.CommentsDelete:
            options.RequireRole("CommentModerator", "Administrator");
            break;
        //all other policies/permission require membership of the Administrator role)
        default:
            options.RequireRole("Administrator");
            break;
    }
});
~~~

We provide two different packages for handling authentication, one for development and one for production scenarios.

To read more about how the implement custom authentication services for your application, please refer to [Authentication](../extensions/authentication) under the Extensions section.

## Adding Permissions

Besides the claims used by the default pages in the manager interface you can add custom claims for you application, both for custom manager pages **or** for securing pages in your application.

### Adding Application Claims

The application claims you add will be available when you edit and set up **Roles** in the manager if you're using the `Identity` package. These claims will also be available in the settings for your Pages & Posts and can be used for securing certain instances of your content. This should be added in your `Startup.cs`.

~~~ csharp
App.Permissions["Application"].Add(new Aero.Security.PermissionItem
{
    Name = "WebUser",
    Title = "Web User"
});
~~~

The first name is the main category you want to group your permissions in and can be anything you like. In this example we've choosen the name "Application".

### Adding Manager Claims

Manager claims works in the same way as application claims, the only difference is that you set the property `IsInternal` to `true`. By doing this they are not shown when specifying permissions for your public pages & posts and should only be used when validating if the current manager should have access to something in the manager interface.

~~~ csharp
App.Permissions["Manager"].Add(new Aero.Security.PermissionItem
{
    Category = "My Manager Feature",
    Name = "EditStuff",
    Title = "Edit Stuff",
    IsInternal = true
});
App.Permissions["Manager"].Add(new Aero.Security.PermissionItem
{
    Category = "My Manager Feature",
    Name = "DeleteStuff",
    Title = "Delete Stuff",
    IsInternal = true
});
~~~

## Core Claims

The core Aero application has two Claims that are used when trying to preview unpublished content.

* `AeroPagePreview`
* `AeroPostPreview`

## Manager Claims

The following claims define the different actions the logged in user can perform in the manager interface. To assign these claims to different users you setup `Roles` which have access to different `Claims`. A user can have several roles.

### Basic

* `AeroAdmin` If the user has access to the manager interface

### Aliases

* `AeroAliases` If the user can view the alias page
* `AeroAliasesDelete` If the user can delete existing aliases
* `AeroAliasesEdit` If the user can add and edit existing aliases

### Config

* `AeroConfig` If the user can view the config page
* `AeroConfigEdit` If the user can update config settings

### Media

* `AeroMedia` If the user can view the media page
* `AeroMediaAdd` If the user can upload new media
* `AeroMediaDelete` If the user can delete existing media
* `AeroMediaEdit` If the user can update existing media
* `AeroMediaAddFolder` If the user can add new folders in the media library
* `AeroMediaDeleteFolder` If the user can delete existing media folders

### Pages

* `AeroPages` If the user can view the page structure
* `AeroPagesAdd` If the user can add new pages
* `AeroPagesDelete` If the user can delete existing pages
* `AeroPagesEdit` If the user can view the page details
* `AeroPagesPublish` If the user can publish and unpublish pages
* `AeroPagesSave` If the user can update existing pages

### Posts

* `AeroPosts` If the user can view posts 
* `AeroPostsAdd` If the user can add new posts
* `AeroPostsDelete` If the user can delete existing posts
* `AeroPostsEdit` If the user can view the post details
* `AeroPostsPublish` If the user can publish and unpublished posts
* `AeroPostsSave` If the user can update existing posts

### Sites

* `AeroSites` If the user can view the site page
* `AeroSitesAdd` If the user can add new sites
* `AeroSitesDelete` If the user can delete existing sites
* `AeroSitesEdit` If the user can view site details
* `AeroSitesSave` If the user can update existing sites
