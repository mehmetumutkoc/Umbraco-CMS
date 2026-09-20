using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed class FuzulVenturesCmsSetup
{
    private readonly IConfigurationEditorJsonSerializer _configurationSerializer;
    private readonly IContentService _contentService;
    private readonly IContentTypeService _contentTypeService;
    private readonly IDataTypeService _dataTypeService;
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IKeyValueService _keyValueService;
    private readonly ILogger<FuzulVenturesCmsSetup> _logger;
    private readonly IMediaImportService _mediaImportService;
    private readonly IMediaService _mediaService;
    private readonly PropertyEditorCollection _propertyEditors;
    private readonly IShortStringHelper _shortStringHelper;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public FuzulVenturesCmsSetup(
        IContentTypeService contentTypeService,
        IContentService contentService,
        IDataTypeService dataTypeService,
        IMediaService mediaService,
        IMediaImportService mediaImportService,
        IKeyValueService keyValueService,
        PropertyEditorCollection propertyEditors,
        IConfigurationEditorJsonSerializer configurationSerializer,
        IJsonSerializer jsonSerializer,
        IShortStringHelper shortStringHelper,
        IWebHostEnvironment webHostEnvironment,
        ILogger<FuzulVenturesCmsSetup> logger)
    {
        _contentTypeService = contentTypeService;
        _contentService = contentService;
        _dataTypeService = dataTypeService;
        _mediaService = mediaService;
        _mediaImportService = mediaImportService;
        _keyValueService = keyValueService;
        _propertyEditors = propertyEditors;
        _configurationSerializer = configurationSerializer;
        _jsonSerializer = jsonSerializer;
        _shortStringHelper = shortStringHelper;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task EnsureAsync(ITemplate template)
    {
        _logger.LogInformation("Fuzul Ventures CMS setup started.");
        MediaLibrary media = await EnsureMediaAsync();
        _logger.LogInformation("Fuzul media library ready with {Count} items.", media.ByName.Count);
        IDataType mediaPicker = await EnsureMediaPickerAsync(media.Folder);
        IDataType richText = await RequireDataTypeAsync(Constants.DataTypes.Guids.RichtextEditorGuid, "Rich Text Editor");
        IDataType urlPicker = await RequireDataTypeAsync(Constants.DataTypes.Guids.RelatedLinksGuid, "Multi URL Picker");

        IContentType sectorElement = await EnsureElementTypeAsync(
            FuzulVenturesAliases.SectorElement,
            "Yatırım alanı",
            "icon-chart-curve color-deep-orange",
            TextBox("title", "Başlık", "Kartta görünen sektör adı.", true),
            TextArea("text", "Açıklama", "Kısa tanıtım metni."),
            Media("image", "Görsel", "Sektör ikonu veya görseli.", mediaPicker));

        IContentType stepElement = await EnsureElementTypeAsync(
            FuzulVenturesAliases.ProcessStepElement,
            "Süreç adımı",
            "icon-ordered-list color-deep-orange",
            TextBox("number", "Numara", "Örneğin 01.", true),
            TextBox("title", "Başlık", "Adımın adı.", true),
            TextArea("text", "Açıklama", "Bu adımda ne olduğunu yazın."),
            Media("image", "Görsel", "Adım ikonu.", mediaPicker));

        IContentType memberElement = await EnsureElementTypeAsync(
            FuzulVenturesAliases.CommitteeMemberElement,
            "Komite üyesi",
            "icon-user color-deep-orange",
            TextBox("displayName", "Ad", "Kartta görünen ad.", true),
            TextBox("modalTitle", "Modal başlığı", "Detay penceresinin başlığı."),
            RichText("bio", "Biyografi", "Üye hakkında metin. Paragraflar ve vurgu kullanabilirsiniz.", richText),
            Media("image", "Fotoğraf", "Komite üyesi fotoğrafı.", mediaPicker));

        IDataType sectorsList = await EnsureBlockListAsync(FuzulVenturesAliases.SectorsDataTypeName, sectorElement);
        IDataType processList = await EnsureBlockListAsync(FuzulVenturesAliases.ProcessDataTypeName, stepElement);
        IDataType committeeList = await EnsureBlockListAsync(FuzulVenturesAliases.CommitteeDataTypeName, memberElement);

        IContentType submission = await EnsureFormSubmissionTypeAsync();
        IContentType formFolder = await EnsureFormFolderTypeAsync(submission);
        IContentType homeType = await EnsureHomeTypeAsync(template, submission, formFolder, mediaPicker, urlPicker, richText, sectorsList, processList, committeeList);

        await EnsureContentAsync(homeType, template, formFolder, media, sectorElement, stepElement, memberElement);
        _logger.LogInformation("Fuzul Ventures CMS setup finished.");
    }

    private async Task<IContentType> EnsureHomeTypeAsync(
        ITemplate template,
        IContentType submission,
        IContentType formFolder,
        IDataType mediaPicker,
        IDataType urlPicker,
        IDataType richText,
        IDataType sectorsList,
        IDataType processList,
        IDataType committeeList)
    {
        IContentType? home = _contentTypeService.Get(FuzulVenturesAliases.Home);
        var created = home is null;
        if (created == false
            && home!.PropertyTypeExists(FuzulVenturesAliases.SectorItems)
            && home.PropertyTypeExists(FuzulVenturesAliases.Logo)
            && home.PropertyGroups.Any(x => x.Type == PropertyGroupType.Tab)
            && home.AllowedContentTypes?.Any(x => x.Alias == formFolder.Alias) == true)
        {
            return home;
        }

        home ??= new ContentType(_shortStringHelper, Constants.System.Root)
        {
            Alias = FuzulVenturesAliases.Home,
            Name = "Fuzul Home",
            Icon = "icon-home color-deep-orange",
            AllowedAsRoot = true,
        };

        home.Name = "Fuzul Home";
        home.Description = "Ana sayfa. Sekmelerden metinleri düzenleyin; yatırım alanları, süreç ve komiteyi blok listesinden ekleyip sıralayın. Form başvuruları altındaki Başvurular klasöründe durur.";
        home.Icon = "icon-home color-deep-orange";
        home.AllowedAsRoot = true;
        home.AllowedTemplates = [template];
        home.SetDefaultTemplate(template);
        home.AllowedContentTypes = [new ContentTypeSort(formFolder.Key, 0, formFolder.Alias)];

        AddProperty(home, Media(FuzulVenturesAliases.Logo, "Logo", "Üst menüde görünen logo. Medya kütüphanesinden seçin veya yükleyin.", mediaPicker), "brand", "Marka");
        AddProperty(home, UrlList(FuzulVenturesAliases.Navigation, "Menü bağlantıları", "Sırayı sürükleyerek değiştirin. Harici URL olarak #kurumsal gibi sayfa içi bağlantılar kullanın.", urlPicker), "nav", "Menü");
        AddProperty(home, TextBox("heroLine1", "Satır 1", "Hero başlığının ilk satırı.", true), "hero", "Hero");
        AddProperty(home, TextBox("heroLine2", "Satır 2", "Hero başlığının ikinci satırı."), "hero", "Hero");
        AddProperty(home, TextBox("heroLine3", "Satır 3", "Hero başlığının üçüncü satırı."), "hero", "Hero");
        AddProperty(home, RichText("heroSubheading", "Alt metin", "Başlığın altındaki kısa açıklama.", richText), "hero", "Hero");
        AddProperty(home, TextBox("heroButton", "Buton metni", "Hero çağrı butonu."), "hero", "Hero");
        AddProperty(home, TextBox("aboutTitle", "Başlık", "Hakkımızda bölümü başlığı."), "about", "Hakkımızda");
        AddProperty(home, RichText("aboutText", "Metin", "Hakkımızda gövde metni.", richText), "about", "Hakkımızda");
        AddProperty(home, TextBox("sectorsPrev", "Geri", "Sektör karuselindeki geri düğmesi."), "sectors", "Yatırım alanları");
        AddProperty(home, TextBox("sectorsNext", "İleri", "Sektör karuselindeki ileri düğmesi."), "sectors", "Yatırım alanları");
        AddProperty(home, BlockList(FuzulVenturesAliases.SectorItems, "Alanlar", "Kart ekleyin, sıralayın veya silin. Her kartın görselini medya kütüphanesinden seçin.", sectorsList), "sectors", "Yatırım alanları");
        AddProperty(home, TextBox("processTitle", "Başlık", "Süreç bölümü başlığı."), "process", "Yatırım süreci");
        AddProperty(home, TextBox("processEyebrow", "Üst etiket", "Başlığın altındaki küçük metin."), "process", "Yatırım süreci");
        AddProperty(home, BlockList(FuzulVenturesAliases.ProcessSteps, "Adımlar", "Yatırım sürecinin adımları.", processList), "process", "Yatırım süreci");
        AddProperty(home, TextBox("committeeTitle", "Başlık", "Komite bölümü başlığı."), "committee", "Yatırım komitesi");
        AddProperty(home, TextBox("committeeEyebrow", "Üst etiket", "Başlığın altındaki küçük metin."), "committee", "Yatırım komitesi");
        AddProperty(home, BlockList(FuzulVenturesAliases.CommitteeMembers, "Üyeler", "Fotoğraf, ad ve biyografi. Biyografi zengin metin alanıdır.", committeeList), "committee", "Yatırım komitesi");
        AddProperty(home, TextBox("applyTitle", "Başlık", "Başvuru bölümü başlığı."), "apply", "Başvuru");
        AddProperty(home, RichText("applyText", "Metin", "Başvuru formu yanındaki açıklama.", richText), "apply", "Başvuru");
        AddProperty(home, TextBox("contactTitle", "Başlık", "İletişim bölümü başlığı."), "contact", "İletişim");
        AddProperty(home, RichText("contactText", "Metin", "İletişim formu yanındaki açıklama.", richText), "contact", "İletişim");
        AddProperty(home, TextBox("email", "E-posta", "Sitede görünen e-posta adresi."), "footer", "Footer");
        AddProperty(home, TextBox("location", "Konum", "Footer’da görünen şehir."), "footer", "Footer");
        AddProperty(home, UrlList(FuzulVenturesAliases.SocialLinks, "Sosyal medya", "Yeni sekmede açılacak sosyal bağlantılar.", urlPicker), "footer", "Footer");
        AddProperty(home, TextBox("copyright", "Telif", "Footer telif metni."), "footer", "Footer");

        ConvertExistingToRichText(home, richText, "heroSubheading", "aboutText", "applyText", "contactText");
        DescribeExisting(home);
        ConfigureTabs(home);

        if (created)
        {
            Attempt<ContentTypeOperationStatus> createdAttempt = await _contentTypeService.CreateAsync(home, Constants.Security.SuperUserKey);
            if (createdAttempt.Success == false)
            {
                throw new InvalidOperationException($"Could not create the Fuzul Home document type: {createdAttempt.Result}");
            }
        }
        else
        {
            Attempt<ContentTypeOperationStatus> updated = await _contentTypeService.UpdateAsync(home, Constants.Security.SuperUserKey);
            if (updated.Success == false)
            {
                throw new InvalidOperationException($"Could not update the Fuzul Home document type: {updated.Result}");
            }
        }

        return _contentTypeService.Get(FuzulVenturesAliases.Home)
               ?? throw new InvalidOperationException("Fuzul Home document type could not be reloaded.");
    }

    private async Task EnsureContentAsync(
        IContentType homeType,
        ITemplate template,
        IContentType formFolderType,
        MediaLibrary media,
        IContentType sectorElement,
        IContentType stepElement,
        IContentType memberElement)
    {
        IContent? home = _contentService.GetRootContent()
            .FirstOrDefault(x => x.ContentType.Alias == FuzulVenturesAliases.Home);

        if (home is null)
        {
            home = _contentService.Create("Fuzul Ventures", Constants.System.Root, homeType);
            home.TemplateId = template.Id;
            SetHomeText(home);
            SetHomePickersAndBlocks(home, media, sectorElement, stepElement, memberElement);
            _contentService.Save(home);
            _contentService.Publish(home, ["*"]);
            EnsureFormFolder(home, formFolderType);
            _keyValueService.SetValue(FuzulVenturesAliases.CmsVersionKey, FuzulVenturesAliases.CmsVersion);
            _logger.LogInformation("Seeded the Fuzul Ventures homepage with block-based editing.");
        }
        else
        {
            home.TemplateId = template.Id;
            EnsureFormFolder(home, formFolderType);

            if (_keyValueService.GetValue(FuzulVenturesAliases.CmsVersionKey) != FuzulVenturesAliases.CmsVersion)
            {
                home = _contentService.GetById(home.Id) ?? home;
                SetHomePickersAndBlocks(home, media, sectorElement, stepElement, memberElement, preferExistingChildren: true);
                _contentService.Save(home);
                _contentService.Publish(home, ["*"]);
                TrashLegacyChildren(home);
                await RemoveLegacyHomePropertiesAsync();
                _keyValueService.SetValue(FuzulVenturesAliases.CmsVersionKey, FuzulVenturesAliases.CmsVersion);
                _logger.LogInformation("Upgraded the Fuzul Ventures backoffice to block lists and media pickers.");
            }
        }

        IContent[] roots = _contentService.GetRootContent().ToArray();
        IContent[] ordered = roots
            .OrderBy(x => x.ContentType.Alias == FuzulVenturesAliases.Home ? 0 : 1)
            .ThenBy(x => x.SortOrder)
            .ToArray();
        _contentService.Sort(ordered);

        foreach (IContent other in roots.Where(x => x.ContentType.Alias != FuzulVenturesAliases.Home && x.Published))
        {
            _contentService.Unpublish(other);
        }
    }

    private void EnsureFormFolder(IContent home, IContentType formFolderType)
    {
        IEnumerable<IContent> children = _contentService.GetPagedChildren(home.Id, 0, 200, out _, propertyAliases: null, filter: null, ordering: null);
        IContent? folder = children.FirstOrDefault(x => x.ContentType.Alias == FuzulVenturesAliases.FormFolder);
        if (folder is null)
        {
            folder = _contentService.Create("Başvurular", home.Key, formFolderType.Alias);
            _contentService.Save(folder);
            _contentService.Publish(folder, ["*"]);
        }

        foreach (IContent submission in children.Where(x => x.ContentType.Alias == FuzulVenturesAliases.FormSubmission))
        {
            _contentService.Move(submission, folder.Id);
        }
    }

    private void TrashLegacyChildren(IContent home)
    {
        IEnumerable<IContent> children = _contentService.GetPagedChildren(home.Id, 0, 200, out _, propertyAliases: null, filter: null, ordering: null);
        foreach (IContent child in children.Where(x =>
                     x.ContentType.Alias is FuzulVenturesAliases.Sector
                         or FuzulVenturesAliases.ProcessStep
                         or FuzulVenturesAliases.CommitteeMember))
        {
            _contentService.MoveToRecycleBin(child);
        }
    }

    private async Task RemoveLegacyHomePropertiesAsync()
    {
        IContentType? home = _contentTypeService.Get(FuzulVenturesAliases.Home);
        if (home is null)
        {
            return;
        }

        string[] obsolete =
        [
            "logoUrl",
            "navAbout",
            "navCommittee",
            "navApply",
            "navMedia",
            "navContact",
            "navLanguage",
            "facebookUrl",
            "instagramUrl",
            "twitterUrl",
            "linkedinUrl",
        ];

        var changed = false;
        foreach (var alias in obsolete)
        {
            if (home.PropertyTypeExists(alias) == false)
            {
                continue;
            }

            home.RemovePropertyType(alias);
            changed = true;
        }

        if (changed)
        {
            await _contentTypeService.UpdateAsync(home, Constants.Security.SuperUserKey);
        }
    }

    private void SetHomeText(IContent home)
    {
        home.SetValue("heroLine1", "Geleceği İnşa Etmenin");
        home.SetValue("heroLine2", "Vizyonuyla Bugüne");
        home.SetValue("heroLine3", "Yapılan Yatırım");
        SetRichText(home, "heroSubheading", "İnovasyonun ve girişimciliğin gücüne inanıyoruz ve vizyon sahibi, potansiyel dolu projeleri desteklemek amacıyla çalışıyoruz.");
        home.SetValue("heroButton", "Yatırım Alanları");
        home.SetValue("aboutTitle", "Seeding Funding");
        SetRichText(
            home,
            "aboutText",
            "Yatırımlarımız, geleceğin lider şirketlerinin temelini oluşturmak için stratejik ve uzun vadeli bir yaklaşımı yansıtır. Başarılı bir yatırım süreci için, girişimcilere sadece finansal destek sağlamakla kalmıyor, aynı zamanda zengin bir deneyim ve değerli bağlantılar sunuyoruz.");
        home.SetValue("sectorsPrev", "Geri");
        home.SetValue("sectorsNext", "İleri");
        home.SetValue("processTitle", "Yatırım Süreci");
        home.SetValue("processEyebrow", "Fuzul Ventures");
        home.SetValue("committeeTitle", "Yatırım Komitesi");
        home.SetValue("committeeEyebrow", "Fuzul Ventures");
        home.SetValue("applyTitle", "BAŞVURU FORMU");
        SetRichText(home, "applyText", "Girişiminizi geleceğe taşımak için bize ulaşın.");
        home.SetValue("contactTitle", "İLETİŞİM");
        SetRichText(home, "contactText", "İletişim formunu doldurarak bizimle iletişime geçebilirsiniz.");
        home.SetValue("email", FuzulVenturesSeedData.Email);
        home.SetValue("location", FuzulVenturesSeedData.Location);
        home.SetValue("copyright", "© 2023 Fuzul Ventures, All Right Reserved.");
    }

    private void SetHomePickersAndBlocks(
        IContent home,
        MediaLibrary media,
        IContentType sectorElement,
        IContentType stepElement,
        IContentType memberElement,
        bool preferExistingChildren = false)
    {
        if (home.HasProperty(FuzulVenturesAliases.Logo) && IsEmpty(home, FuzulVenturesAliases.Logo))
        {
            home.SetValue(FuzulVenturesAliases.Logo, MediaPickerValue(media.Find(FuzulVenturesSeedData.LogoUrl)));
        }

        if (home.HasProperty(FuzulVenturesAliases.Navigation) && IsEmpty(home, FuzulVenturesAliases.Navigation))
        {
            home.SetValue(FuzulVenturesAliases.Navigation, SerializeLinks(FuzulVenturesSeedData.Navigation.Select(x => (x.Label, x.Url, (string?)null))));
        }

        if (home.HasProperty(FuzulVenturesAliases.SocialLinks) && IsEmpty(home, FuzulVenturesAliases.SocialLinks))
        {
            home.SetValue(FuzulVenturesAliases.SocialLinks, SerializeLinks(FuzulVenturesSeedData.Social.Select(x => (x.Label, x.Url, (string?)"_blank"))));
        }

        if (home.HasProperty(FuzulVenturesAliases.SectorItems) && IsEmpty(home, FuzulVenturesAliases.SectorItems))
        {
            IEnumerable<IContent> children = preferExistingChildren
                ? _contentService.GetPagedChildren(home.Id, 0, 200, out _, propertyAliases: null, filter: null, ordering: null).Where(x => x.ContentType.Alias == FuzulVenturesAliases.Sector)
                : [];

            var items = children.Any()
                ? children.Select(x => BlockValues(
                    ("title", x.GetValue<string>("title")),
                    ("text", x.GetValue<string>("text")),
                    ("image", MediaPickerValue(media.Find(x.GetValue<string>("imageUrl"))))))
                : FuzulVenturesSeedData.Sectors.Select(x => BlockValues(
                    ("title", x.Title),
                    ("text", x.Text),
                    ("image", MediaPickerValue(media.Find(x.ImageUrl)))));

            home.SetValue(FuzulVenturesAliases.SectorItems, SerializeBlocks(sectorElement, items));
        }

        if (home.HasProperty(FuzulVenturesAliases.ProcessSteps) && IsEmpty(home, FuzulVenturesAliases.ProcessSteps))
        {
            IEnumerable<IContent> children = preferExistingChildren
                ? _contentService.GetPagedChildren(home.Id, 0, 200, out _, propertyAliases: null, filter: null, ordering: null).Where(x => x.ContentType.Alias == FuzulVenturesAliases.ProcessStep)
                : [];

            var items = children.Any()
                ? children.Select(x => BlockValues(
                    ("number", x.GetValue<string>("number")),
                    ("title", x.GetValue<string>("title")),
                    ("text", x.GetValue<string>("text")),
                    ("image", MediaPickerValue(media.Find(x.GetValue<string>("imageUrl"))))))
                : FuzulVenturesSeedData.ProcessSteps.Select(x => BlockValues(
                    ("number", x.Number),
                    ("title", x.Title),
                    ("text", x.Text),
                    ("image", MediaPickerValue(media.Find(x.ImageUrl)))));

            home.SetValue(FuzulVenturesAliases.ProcessSteps, SerializeBlocks(stepElement, items));
        }

        if (home.HasProperty(FuzulVenturesAliases.CommitteeMembers) && IsEmpty(home, FuzulVenturesAliases.CommitteeMembers))
        {
            IEnumerable<IContent> children = preferExistingChildren
                ? _contentService.GetPagedChildren(home.Id, 0, 200, out _, propertyAliases: null, filter: null, ordering: null).Where(x => x.ContentType.Alias == FuzulVenturesAliases.CommitteeMember)
                : [];

            var items = children.Any()
                ? children.Select(x => BlockValues(
                    ("displayName", x.GetValue<string>("displayName") ?? x.Name),
                    ("modalTitle", x.GetValue<string>("modalTitle")),
                    ("bio", RichTextMarkup(x.GetValue<string>("bio"))),
                    ("image", MediaPickerValue(media.Find(x.GetValue<string>("imageUrl"))))))
                : FuzulVenturesSeedData.Members.Select(x => BlockValues(
                    ("displayName", x.Name),
                    ("modalTitle", x.ModalTitle),
                    ("bio", RichTextMarkup(x.Bio)),
                    ("image", MediaPickerValue(media.Find(x.ImageUrl)))));

            home.SetValue(FuzulVenturesAliases.CommitteeMembers, SerializeBlocks(memberElement, items));
        }
    }

    private async Task<IContentType> EnsureFormSubmissionTypeAsync()
    {
        IContentType? existing = _contentTypeService.Get(FuzulVenturesAliases.FormSubmission);
        if (existing is not null)
        {
            return existing;
        }

        return await CreateDocumentTypeAsync(
            FuzulVenturesAliases.FormSubmission,
            "Form başvurusu",
            "icon-mailbox color-deep-orange",
            "Siteden gelen başvuru ve iletişim kayıtları. Yayınlanmaz.",
            false,
            TextBox("kind", "Tür", "apply veya contact."),
            TextArea("payload", "İçerik", "Gönderilen form alanları."),
            TextBox("filePath", "Dosya", "Yüklenen PDF’nin yolu."));
    }

    private async Task<IContentType> EnsureFormFolderTypeAsync(IContentType submission)
    {
        IContentType? existing = _contentTypeService.Get(FuzulVenturesAliases.FormFolder);
        if (existing is not null)
        {
            existing.ListView = Constants.DataTypes.Guids.ListViewContentGuid;
            existing.AllowedContentTypes = [new ContentTypeSort(submission.Key, 0, submission.Alias)];
            await _contentTypeService.UpdateAsync(existing, Constants.Security.SuperUserKey);
            return _contentTypeService.Get(FuzulVenturesAliases.FormFolder) ?? existing;
        }

        var type = new ContentType(_shortStringHelper, Constants.System.Root)
        {
            Alias = FuzulVenturesAliases.FormFolder,
            Name = "Form başvuruları",
            Description = "Başvuru ve iletişim formlarından gelen kayıtların listesi.",
            Icon = "icon-folder color-deep-orange",
            AllowedAsRoot = false,
            ListView = Constants.DataTypes.Guids.ListViewContentGuid,
            AllowedContentTypes = [new ContentTypeSort(submission.Key, 0, submission.Alias)],
        };

        Attempt<ContentTypeOperationStatus> created = await _contentTypeService.CreateAsync(type, Constants.Security.SuperUserKey);
        if (created.Success == false)
        {
            throw new InvalidOperationException($"Could not create form folder type: {created.Result}");
        }

        return _contentTypeService.Get(FuzulVenturesAliases.FormFolder)
               ?? throw new InvalidOperationException("Form folder type was created but could not be reloaded.");
    }

    private async Task<IContentType> EnsureElementTypeAsync(string alias, string name, string icon, params IPropertyType[] properties)
    {
        IContentType? existing = _contentTypeService.Get(alias);
        if (existing is not null)
        {
            return existing;
        }

        var type = new ContentType(_shortStringHelper, Constants.System.Root)
        {
            Alias = alias,
            Name = name,
            Icon = icon,
            IsElement = true,
            AllowedInLibrary = true,
            AllowedAsRoot = false,
        };

        foreach (IPropertyType property in properties)
        {
            type.AddPropertyType(property, "content", "İçerik");
        }

        Attempt<ContentTypeOperationStatus> created = await _contentTypeService.CreateAsync(type, Constants.Security.SuperUserKey);
        if (created.Success == false)
        {
            throw new InvalidOperationException($"Could not create element type '{alias}': {created.Result}");
        }

        return _contentTypeService.Get(alias)
               ?? throw new InvalidOperationException($"Element type '{alias}' was created but could not be reloaded.");
    }

    private async Task<IContentType> CreateDocumentTypeAsync(
        string alias,
        string name,
        string icon,
        string description,
        bool allowedAsRoot,
        params IPropertyType[] properties)
    {
        var type = new ContentType(_shortStringHelper, Constants.System.Root)
        {
            Alias = alias,
            Name = name,
            Icon = icon,
            Description = description,
            AllowedAsRoot = allowedAsRoot,
        };

        foreach (IPropertyType property in properties)
        {
            type.AddPropertyType(property, "content", "İçerik");
        }

        Attempt<ContentTypeOperationStatus> created = await _contentTypeService.CreateAsync(type, Constants.Security.SuperUserKey);
        if (created.Success == false)
        {
            throw new InvalidOperationException($"Could not create document type '{alias}': {created.Result}");
        }

        return _contentTypeService.Get(alias)
               ?? throw new InvalidOperationException($"Document type '{alias}' was created but could not be reloaded.");
    }

    private async Task<IDataType> EnsureMediaPickerAsync(IMedia? folder)
    {
        IDataType? existing = await _dataTypeService.GetAsync(FuzulVenturesAliases.MediaPickerName);
        var configuration = new Dictionary<string, object>
        {
            ["multiple"] = false,
            ["enableLocalFocalPoint"] = true,
            ["validationLimit"] = new MediaPicker3Configuration.NumberRange { Min = 0, Max = 1 },
        };
        if (folder is not null)
        {
            configuration["startNodeId"] = folder.Key;
        }

        if (existing is not null)
        {
            existing.ConfigurationData = configuration;
            existing.EditorUiAlias = "Umb.PropertyEditorUi.MediaPicker";
            await _dataTypeService.UpdateAsync(existing, Constants.Security.SuperUserKey);
            return await _dataTypeService.GetAsync(FuzulVenturesAliases.MediaPickerName) ?? existing;
        }

        IDataEditor editor = RequireEditor(Constants.PropertyEditors.Aliases.MediaPicker3);
        var dataType = new DataType(editor, _configurationSerializer)
        {
            Name = FuzulVenturesAliases.MediaPickerName,
            DatabaseType = ValueStorageType.Ntext,
            EditorUiAlias = "Umb.PropertyEditorUi.MediaPicker",
            ConfigurationData = configuration,
        };

        Attempt<IDataType, DataTypeOperationStatus> created = await _dataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        if (created.Success == false || created.Result is null)
        {
            throw new InvalidOperationException($"Could not create media picker: {created.Status}");
        }

        return created.Result;
    }

    private async Task<IDataType> EnsureBlockListAsync(string name, IContentType elementType)
    {
        IDataType? existing = await _dataTypeService.GetAsync(name);
        if (existing is not null)
        {
            return existing;
        }

        IDataEditor editor = RequireEditor(Constants.PropertyEditors.Aliases.BlockList);
        var dataType = new DataType(editor, _configurationSerializer)
        {
            Name = name,
            DatabaseType = ValueStorageType.Ntext,
            EditorUiAlias = "Umb.PropertyEditorUi.BlockList",
            ConfigurationData = new Dictionary<string, object>
            {
                ["blocks"] = new BlockListConfiguration.BlockConfiguration[]
                {
                    new() { ContentElementTypeKey = elementType.Key },
                },
            },
        };

        Attempt<IDataType, DataTypeOperationStatus> created = await _dataTypeService.CreateAsync(dataType, Constants.Security.SuperUserKey);
        if (created.Success == false || created.Result is null)
        {
            throw new InvalidOperationException($"Could not create block list '{name}': {created.Status}");
        }

        return created.Result;
    }

    private async Task<IDataType> RequireDataTypeAsync(Guid key, string fallbackName)
        => await _dataTypeService.GetAsync(key)
           ?? await _dataTypeService.GetAsync(fallbackName)
           ?? throw new InvalidOperationException($"Built-in data type '{fallbackName}' was not found.");

    private IDataEditor RequireEditor(string alias)
        => _propertyEditors[alias] ?? throw new InvalidOperationException($"Property editor '{alias}' is not registered.");

    private async Task<MediaLibrary> EnsureMediaAsync()
    {
        IMedia folder = _mediaService.GetRootMedia()
                            .FirstOrDefault(x => string.Equals(x.Name, "Fuzul Ventures", StringComparison.OrdinalIgnoreCase))
                        ?? CreateFolder();

        var imported = new Dictionary<string, IMedia>(StringComparer.OrdinalIgnoreCase);
        foreach (IMedia child in _mediaService.GetPagedChildren(folder.Id, 0, 200, out _))
        {
            IndexMedia(imported, child);
        }

        var imagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "fuzul", "images");
        if (Directory.Exists(imagesPath) == false)
        {
            _logger.LogWarning("Fuzul image folder was not found at {Path}. Media picker items will be empty until files are copied.", imagesPath);
            return new MediaLibrary(folder, imported);
        }

        foreach (var filePath in Directory.GetFiles(imagesPath))
        {
            var fileName = Path.GetFileName(filePath);
            if (imported.ContainsKey(Path.GetFileNameWithoutExtension(fileName)) || imported.ContainsKey(fileName))
            {
                continue;
            }

            try
            {
                IMedia media = await ImportMediaAsync(filePath, fileName, folder.Key);
                IndexMedia(imported, media, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not import {File} into the Fuzul media folder.", fileName);
            }
        }

        return new MediaLibrary(folder, imported);
    }

    private IMedia CreateFolder()
    {
        IMedia folder = _mediaService.CreateMedia("Fuzul Ventures", Constants.System.Root, Constants.Conventions.MediaTypes.Folder);
        _mediaService.Save(folder);
        return folder;
    }

    private async Task<IMedia> ImportMediaAsync(string filePath, string fileName, Guid folderKey)
    {
        var bytes = await System.IO.File.ReadAllBytesAsync(filePath);
        try
        {
            return await ImportBytesAsync(fileName, bytes, folderKey, MediaTypeAlias(fileName));
        }
        catch (Exception ex) when (fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogDebug(ex, "Vector import failed for {File}, retrying as a file.", fileName);
            return await ImportBytesAsync(fileName, bytes, folderKey, Constants.Conventions.MediaTypes.File);
        }
    }

    private async Task<IMedia> ImportBytesAsync(string fileName, byte[] bytes, Guid folderKey, string mediaTypeAlias)
    {
        using var stream = new MemoryStream(bytes);
        return await _mediaImportService.ImportAsync(
            fileName,
            stream,
            folderKey,
            mediaTypeAlias,
            Constants.Security.SuperUserKey);
    }

    private static void IndexMedia(Dictionary<string, IMedia> imported, IMedia media, string? fileName = null)
    {
        if (string.IsNullOrWhiteSpace(media.Name) == false)
        {
            imported[media.Name] = media;
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return;
        }

        imported[fileName] = media;
        imported[Path.GetFileNameWithoutExtension(fileName)] = media;
    }

    private static string MediaTypeAlias(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (extension.Equals(".svg", StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Conventions.MediaTypes.VectorGraphicsAlias;
        }

        if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".jpeg", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".webp", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".gif", StringComparison.OrdinalIgnoreCase))
        {
            return Constants.Conventions.MediaTypes.Image;
        }

        return Constants.Conventions.MediaTypes.File;
    }

    private IPropertyType TextBox(string alias, string name, string description, bool mandatory = false)
        => Property(alias, name, description, Constants.DataTypes.Textbox, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Nvarchar, mandatory, labelOnTop: false);

    private IPropertyType TextArea(string alias, string name, string description)
        => Property(alias, name, description, Constants.DataTypes.Textarea, Constants.PropertyEditors.Aliases.TextArea, ValueStorageType.Ntext, false, true);

    private IPropertyType RichText(string alias, string name, string description, IDataType dataType)
        => Property(alias, name, description, dataType, false, true);

    private IPropertyType Media(string alias, string name, string description, IDataType dataType)
        => Property(alias, name, description, dataType, false, true);

    private IPropertyType BlockList(string alias, string name, string description, IDataType dataType)
        => Property(alias, name, description, dataType, false, true);

    private IPropertyType UrlList(string alias, string name, string description, IDataType dataType)
        => Property(alias, name, description, dataType, false, true);

    private IPropertyType Property(string alias, string name, string description, IDataType dataType, bool mandatory, bool labelOnTop)
        => new PropertyType(_shortStringHelper, dataType, alias)
        {
            Name = name,
            Description = description,
            Mandatory = mandatory,
            LabelOnTop = labelOnTop,
        };

    private IPropertyType Property(
        string alias,
        string name,
        string description,
        int dataTypeId,
        string editorAlias,
        ValueStorageType storage,
        bool mandatory,
        bool labelOnTop)
        => new PropertyType(_shortStringHelper, editorAlias, storage, alias)
        {
            Name = name,
            Description = description,
            DataTypeId = dataTypeId,
            Mandatory = mandatory,
            LabelOnTop = labelOnTop,
        };

    private static void AddProperty(IContentType type, IPropertyType property, string groupAlias, string groupName)
    {
        if (type.PropertyTypeExists(property.Alias))
        {
            IPropertyType existing = type.PropertyTypes.First(x => x.Alias == property.Alias);
            existing.Name = property.Name;
            existing.Description = property.Description;
            existing.Mandatory = property.Mandatory;
            existing.LabelOnTop = property.LabelOnTop;
            return;
        }

        type.AddPropertyType(property, groupAlias, groupName);
    }

    private static void ConvertExistingToRichText(IContentType home, IDataType richText, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            IPropertyType? property = home.PropertyTypes.FirstOrDefault(x => x.Alias == alias);
            if (property is null)
            {
                continue;
            }

            property.DataTypeId = richText.Id;
            property.DataTypeKey = richText.Key;
            property.PropertyEditorAlias = richText.EditorAlias;
            property.ValueStorageType = ValueStorageType.Ntext;
            property.LabelOnTop = true;
        }
    }

    private static void DescribeExisting(IContentType home)
    {
        Dictionary<string, string> descriptions = new()
        {
            ["heroLine1"] = "Hero başlığının ilk satırı.",
            ["heroLine2"] = "Hero başlığının ikinci satırı.",
            ["heroLine3"] = "Hero başlığının üçüncü satırı.",
            ["heroButton"] = "Hero çağrı butonu.",
            ["aboutTitle"] = "Hakkımızda bölümü başlığı.",
            ["sectorsPrev"] = "Sektör karuselindeki geri düğmesi.",
            ["sectorsNext"] = "Sektör karuselindeki ileri düğmesi.",
            ["processTitle"] = "Süreç bölümü başlığı.",
            ["processEyebrow"] = "Başlığın altındaki küçük metin.",
            ["committeeTitle"] = "Komite bölümü başlığı.",
            ["committeeEyebrow"] = "Başlığın altındaki küçük metin.",
            ["applyTitle"] = "Başvuru bölümü başlığı.",
            ["contactTitle"] = "İletişim bölümü başlığı.",
            ["email"] = "Sitede görünen e-posta adresi.",
            ["location"] = "Footer’da görünen şehir.",
            ["copyright"] = "Footer telif metni.",
        };

        foreach (IPropertyType property in home.PropertyTypes)
        {
            if (descriptions.TryGetValue(property.Alias, out var description))
            {
                property.Description = description;
            }
        }
    }

    private static void ConfigureTabs(IContentType home)
    {
        string[] order = ["brand", "nav", "hero", "about", "sectors", "process", "committee", "apply", "contact", "footer"];
        for (var i = 0; i < order.Length; i++)
        {
            PropertyGroup? group = home.PropertyGroups.FirstOrDefault(x => x.Alias == order[i]);
            if (group is null)
            {
                continue;
            }

            group.Type = PropertyGroupType.Tab;
            group.SortOrder = i;
        }
    }

    private void SetRichText(IContent content, string alias, string text)
    {
        if (content.HasProperty(alias) == false)
        {
            return;
        }

        content.SetValue(alias, RichTextMarkup(text));
    }

    private string RichTextMarkup(string? text)
    {
        var markup = ToHtmlParagraphs(text);
        return RichTextPropertyEditorHelper.SerializeRichTextEditorValue(
            new RichTextEditorValue { Markup = markup, Blocks = null },
            _jsonSerializer);
    }

    private static string ToHtmlParagraphs(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (text.Contains('<'))
        {
            return text;
        }

        var parts = text.Split(["\r\n\r\n", "\n\n"], StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(part => $"<p>{System.Net.WebUtility.HtmlEncode(part.Trim()).Replace("\n", "<br/>", StringComparison.Ordinal)}</p>"));
    }

    private string SerializeBlocks(IContentType elementType, IEnumerable<IList<BlockPropertyValue>> items)
    {
        var layouts = new List<BlockListLayoutItem>();
        var contentData = new List<BlockItemData>();
        var expose = new List<BlockItemVariation>();

        foreach (IList<BlockPropertyValue> values in items)
        {
            var key = Guid.NewGuid();
            layouts.Add(new BlockListLayoutItem(key));
            contentData.Add(new BlockItemData(key, elementType.Key, elementType.Alias) { Values = values });
            expose.Add(new BlockItemVariation(key, null, null));
        }

        return _jsonSerializer.Serialize(new BlockListValue(layouts)
        {
            ContentData = contentData,
            Expose = expose,
        });
    }

    private static IList<BlockPropertyValue> BlockValues(params (string Alias, object? Value)[] values)
        => values.Select(x => new BlockPropertyValue { Alias = x.Alias, Value = x.Value }).ToList();

    private string? MediaPickerValue(IMedia? media)
    {
        if (media is null)
        {
            return null;
        }

        return _jsonSerializer.Serialize(new[]
        {
            new
            {
                key = Guid.NewGuid(),
                mediaKey = media.Key,
                mediaTypeAlias = media.ContentType.Alias,
                crops = Array.Empty<object>(),
                focalPoint = (object?)null,
            },
        });
    }

    private string SerializeLinks(IEnumerable<(string Name, string Url, string? Target)> links)
        => _jsonSerializer.Serialize(links.Select(link => new
        {
            name = link.Name,
            url = link.Url,
            type = "external",
            target = link.Target,
        }));

    private static bool IsEmpty(IContent content, string alias)
    {
        object? value = content.GetValue(alias);
        var text = value?.ToString();
        return string.IsNullOrWhiteSpace(text) || text is "[]" or "{}";
    }

    private sealed record MediaLibrary(IMedia Folder, IReadOnlyDictionary<string, IMedia> ByName)
    {
        public IMedia? Find(string? pathOrName)
        {
            if (string.IsNullOrWhiteSpace(pathOrName))
            {
                return null;
            }

            var fileName = Path.GetFileName(pathOrName);
            var withoutExtension = Path.GetFileNameWithoutExtension(fileName);

            if (ByName.TryGetValue(fileName, out IMedia? byFileName))
            {
                return byFileName;
            }

            if (ByName.TryGetValue(withoutExtension, out IMedia? byName))
            {
                return byName;
            }

            return ByName.Values.FirstOrDefault(x =>
                string.Equals(x.Name, withoutExtension, StringComparison.OrdinalIgnoreCase)
                || string.Equals(x.Name, fileName, StringComparison.OrdinalIgnoreCase)
                || (x.Name?.Contains(withoutExtension, StringComparison.OrdinalIgnoreCase) ?? false));
        }
    }
}
