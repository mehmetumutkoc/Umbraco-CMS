# Fuzul Ventures

The public site is adapted from https://fuzulventures.com/ into this Umbraco application. Original layout, imagery, fonts, carousel and animations live in `wwwroot/fuzul`. The renderer is `Views/fuzulHome.cshtml` plus section partials in `Views/Partials/Fuzul`.

Run from the repository root:

```powershell
dotnet run --project src/Umbraco.Web.UI/Umbraco.Web.UI.csproj
```

The launch profile serves http://localhost:11000. The backoffice is at `/umbraco`.

On startup the Fuzul handler creates the home document type, imports local images into the media library, and seeds navigation, investment sectors, process steps, committee biographies and social links. After that it enables the page builder: homepage and child **Esnek sayfa** documents are assembled from blocks (hero, text, sectors, process, committee, forms, image+text, cards, FAQ). Section order, copy, images, colours, fonts and SEO fields are edited in the backoffice. Existing content is migrated once into builder blocks and then left alone.

Application and contact forms post to `FuzulFormsController`. Submissions are stored under the homepage **Başvurular** folder. PDF uploads accept up to 10 MB. No email delivery is configured.

Local SQLite and the development administrator live in the ignored `appsettings.Local.json` (`Umbraco.CMS.Unattended`) and must stay out of source control.

A live visual editor (Wix-style) is available on the public page. In Development anyone can click **Düzenle** at the bottom left, drag existing buttons/headings, add free-floating widgets, and save. Production requires a backoffice login. Layout JSON is stored on the page as `designerLayout`. Backoffice still owns structured sections; the canvas block **13 · Serbest tuval** is an empty stage for free placement.

The reference site’s EN link does not provide an English version; translation remains a separate content task.
