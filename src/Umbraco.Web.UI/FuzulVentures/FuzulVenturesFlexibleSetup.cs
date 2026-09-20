using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.UI.FuzulVentures;

public sealed partial class FuzulVenturesCmsSetup
{
    private const string FlexiblePageAlias = "fuzulPage";
    private const string FlexibleMigrationKey = "FuzulVentures.FlexibleBuilder.v1";

    private async Task EnsureFlexibleAsync(ITemplate template, IDataType mediaPicker, IDataType richText, IDataType links,
        IDataType sectors, IDataType process, IDataType committee, MediaLibrary media)
    {
        IDataType alignment = await EnsureChoiceAsync("Fuzul Hizalama", "Sol", "Orta", "Sağ");
        IDataType spacing = await EnsureChoiceAsync("Fuzul Bölüm boşluğu", "Yok", "Dar", "Normal", "Geniş");
        IDataType width = await EnsureChoiceAsync("Fuzul İçerik genişliği", "Normal", "Dar", "Tam genişlik");
        IDataType fonts = await EnsureChoiceAsync("Fuzul Yazı tipi", "Syne", "DM Sans", "Arial", "Georgia", "System");
        IDataType columns = await EnsureChoiceAsync("Fuzul Sütun sayısı", "1", "2", "3", "4", "5");
        IDataType imagePosition = await EnsureChoiceAsync("Fuzul Görsel konumu", "Sol", "Sağ");
        IDataType cardLayout = await EnsureChoiceAsync("Fuzul Kart düzeni", "Izgara", "Karusel");

        IContentType settings = await EnsureElementTypeAsync("fuzulSectionSettings", "Bölüm görünümü", "icon-settings",
            Toggle("hidden", "Bölümü gizle", "İçeriği silmeden yayından kaldırır."),
            Toggle("hideMobile", "Mobilde gizle", "768 pikselden küçük ekranlarda gizler."),
            Toggle("hideDesktop", "Masaüstünde gizle", "768 piksel ve üzeri ekranlarda gizler."),
            TextBox("anchor", "Menü bağlantısı / bölüm kimliği", "Örnek: kurumsal. Menüde #kurumsal kullanın. Tekrarlanan kimliklere otomatik sıra eki eklenir."),
            Choice("alignment", "Hizalama", alignment), Choice("spacing", "Üst / alt boşluk", spacing), Choice("width", "Genişlik", width),
            Color("backgroundColor", "Arka plan rengi"), Color("textColor", "Metin rengi"),
            Media("backgroundImage", "Arka plan görseli", "Renk üzerine yerleştirilir. Boş bırakırsanız bölümün varsayılan görünümü kullanılır.", mediaPicker),
            TextBox("imageAlt", "Arka plan açıklaması", "Dekoratif arka planlar yerine anlam taşıyan görselleri görsel–metin bölümünde kullanın."));

        IContentType card = await EnsureElementTypeAsync("fuzulCard", "Kart / istatistik / logo", "icon-pictures",
            TextBox("title", "Başlık / değer", "Kart başlığı veya 100+ gibi istatistik değeri."),
            TextArea("text", "Açıklama", "Kısa açıklama."), Media("image", "Görsel", "Kart veya marka görseli.", mediaPicker),
            TextBox("imageAlt", "Görsel açıklaması", "Ekran okuyucular için."), UrlList("links", "Bağlantı", "İsteğe bağlı kart bağlantısı.", links));
        IDataType cards = await EnsureBlockListAsync("Fuzul Kartlar", card);
        IContentType question = await EnsureElementTypeAsync("fuzulQuestion", "Soru ve cevap", "icon-help-alt",
            TextBox("title", "Soru", "Açılır panel başlığı.", true), RichText("text", "Cevap", "Biçimlendirilmiş yanıt.", richText));
        IDataType questions = await EnsureBlockListAsync("Fuzul SSS soruları", question);

        var types = new Dictionary<string, IContentType>();
        async Task Add(string alias, string name, string icon, params IPropertyType[] fields)
            => types[alias] = await EnsureElementTypeAsync(alias, name, icon, fields);
        await Add("fuzulHeroSection", "01 · Karşılama / Hero", "icon-presentation",
            TextBox("heroLine1", "Başlık — satır 1", "İlk büyük başlık satırı."), TextBox("heroLine2", "Başlık — satır 2", "İsteğe bağlı."),
            TextBox("heroLine3", "Başlık — satır 3", "İsteğe bağlı."), RichText("heroSubheading", "Açıklama", "Başlığın altındaki metin.", richText),
            Media("image", "Arka plan görseli", "Boş ise özgün görsel gösterilir.", mediaPicker),
            UrlList("links", "Butonlar", "İstediğiniz sayfa veya bölüme birden fazla buton ekleyin.", links));
        await Add("fuzulTextSection", "02 · Zengin metin", "icon-document",
            TextBox("title", "Başlık", "İsteğe bağlı başlık."), RichText("text", "Metin", "Metin, listeler ve bağlantılar.", richText));
        await Add("fuzulSectorsSection", "03 · Yatırım alanları", "icon-chart-curve",
            TextBox("title", "Başlık", "İsteğe bağlı."), TextBox("sectorsPrev", "Geri butonu", "Boşsa Geri."), TextBox("sectorsNext", "İleri butonu", "Boşsa İleri."),
            Choice("layout", "Gösterim", cardLayout), Choice("columns", "Masaüstü sütun sayısı", columns), BlockList("sectorItems", "Alanlar", "Kart ekleyin, silin veya sıralayın.", sectors));
        await Add("fuzulProcessSection", "04 · Süreç adımları", "icon-ordered-list",
            TextBox("processTitle", "Başlık", "Bölüm başlığı."), TextBox("processEyebrow", "Alt başlık", "Küçük etiket."),
            BlockList("processSteps", "Adımlar", "Adımları sürükleyerek sıralayın.", process));
        await Add("fuzulCommitteeSection", "05 · Ekip / komite", "icon-users",
            TextBox("committeeTitle", "Başlık", "Bölüm başlığı."), TextBox("committeeEyebrow", "Alt başlık", "Küçük etiket."),
            Choice("columns", "Masaüstü sütun sayısı", columns), BlockList("committeeMembers", "Üyeler", "Fotoğraf, ad, biyografi.", committee));
        foreach (string kind in new[] { "Apply", "Contact" })
        {
            var fields = new List<IPropertyType>
            {
                TextBox("title", "Başlık", "Form başlığı."), RichText("text", "Açıklama", "Form yanındaki metin.", richText),
                TextBox("buttonLabel", "Gönder butonu", "Buton üzerindeki metin."), TextBox("successMessage", "Başarı mesajı", "Başarıyla kaydedilince gösterilir."),
                TextBox("uploadLabel", "Dosya alanı etiketi", "Dosya yükleme açıklaması."), TextBox("uploadHelp", "Dosya yardım metni", "PDF, en fazla 10 MB."),
                Toggle("hideSocial", "Sosyal bağlantıları gizle", "Bu formun yanındaki sosyal bağlantıları gizler."),
                TextBox("email", "İletişim e-postası", "Boşsa site e-postası."), RichText("privacyText", "Aydınlatma metni", "Form altında gösterilir; politika sayfanıza bağlantı ekleyebilirsiniz.", richText),
            };
            string[] fieldNames = kind == "Apply" ? ["StartupName", "Website", "Summary", "Industry", "FounderName", "FounderEmail"] : ["FullName", "Phone", "Email"];
            string[] labels = kind == "Apply" ? ["Girişimin adı", "Web adresi", "Girişim özeti", "Faaliyet alanı", "Kurucunun adı", "Kurucunun e-postası"] : ["Ad soyad", "Telefon", "E-posta"];
            for (var i = 0; i < fieldNames.Length; i++) fields.Add(TextBox("label" + fieldNames[i], labels[i] + " etiketi", "Boşsa varsayılan metin kullanılır."));
            if (kind == "Apply") fields.Add(TextArea("industryOptions", "Faaliyet alanı seçenekleri", "Her satıra bir seçenek yazın."));
            await Add("fuzul" + kind + "Section", kind == "Apply" ? "06 · Başvuru formu" : "07 · İletişim formu", "icon-message", fields.ToArray());
        }
        await Add("fuzulImageTextSection", "08 · Görsel ve metin", "icon-picture",
            TextBox("title", "Başlık", "Bölüm başlığı."), RichText("text", "Metin", "Açıklama.", richText), Media("image", "Görsel", "Ana görsel.", mediaPicker),
            TextBox("imageAlt", "Görsel açıklaması", "Görselin anlamını anlatın."), Choice("imagePosition", "Görselin konumu", imagePosition), UrlList("links", "Butonlar", "İsteğe bağlı.", links));
        await Add("fuzulCtaSection", "09 · Çağrı / buton alanı", "icon-link",
            TextBox("title", "Başlık", "Çağrı başlığı."), RichText("text", "Metin", "Açıklama.", richText), UrlList("links", "Butonlar", "Hedef sayfa veya bağlantılar.", links));
        await Add("fuzulCardsSection", "10 · Kartlar / logolar", "icon-grid",
            TextBox("title", "Başlık", "Bölüm başlığı."), Choice("columns", "Masaüstü sütun sayısı", columns), BlockList("items", "Kartlar", "Logo şeridi, hizmetler veya proje kartları oluşturun.", cards));
        await Add("fuzulStatsSection", "11 · İstatistikler", "icon-chart",
            TextBox("title", "Başlık", "İsteğe bağlı."), Choice("columns", "Masaüstü sütun sayısı", columns), BlockList("items", "Değerler", "Başlık alanına sayı, açıklama alanına etiketi yazın.", cards));
        await Add("fuzulFaqSection", "12 · Sık sorulan sorular", "icon-help-alt",
            TextBox("title", "Başlık", "Bölüm başlığı."), BlockList("items", "Sorular", "Soruları ekleyin ve sıralayın.", questions));
        await Add("fuzulCanvasSection", "13 · Serbest tuval", "icon-brush",
            TextBox("title", "İç not", "Ziyaretçiler görmez; backoffice’de bölümü ayırt etmek için."),
            NumericText("minHeight", "Yükseklik (px)", "200–1200"));

        IDataType builder = await EnsureBuilderDataTypeAsync(types.Values, settings);
        IContentType home = _contentTypeService.Get(FuzulVenturesAliases.Home)!;
        IContentType? page = _contentTypeService.Get(FlexiblePageAlias);
        var createPage = page is null;
        page ??= new ContentType(_shortStringHelper, Constants.System.Root) { Alias = FlexiblePageAlias, Name = "Esnek sayfa", Icon = "icon-document", AllowedAsRoot = false };
        foreach (IContentType type in new[] { home, page })
        {
            AddProperty(type, BlockList("pageSections", "Sayfa bölümleri", "Bölüm ekleyin; sürükleyerek sıralayın, çoğaltın veya silin. Bölümün Ayarlar sekmesinde görünümünü değiştirin.", builder), "builder", "Sayfa oluşturucu");
            AddProperty(type, TextArea("designerLayout", "Görsel tasarım (JSON)", "Canlı tasarımcının kaydettiği serbest yerleşim. Elle düzenlemeyin."), "builder", "Sayfa oluşturucu");
            AddProperty(type, TextBox("seoTitle", "Sayfa başlığı", "Tarayıcı sekmesi ve arama sonucu. Boşsa sayfa adı."), "seo", "SEO ve paylaşım");
            AddProperty(type, TextArea("seoDescription", "Meta açıklaması", "Arama sonucu ve sosyal paylaşım açıklaması."), "seo", "SEO ve paylaşım");
            AddProperty(type, Media("shareImage", "Paylaşım görseli", "Sosyal medya önizleme görseli.", mediaPicker), "seo", "SEO ve paylaşım");
            AddProperty(type, TextBox("canonicalUrl", "Canonical URL", "Boşsa sayfanın kendi adresi. Sadece http/https adresleri."), "seo", "SEO ve paylaşım");
            AddProperty(type, Toggle("noIndex", "Arama motorlarından gizle", "noindex, nofollow meta etiketi ekler."), "seo", "SEO ve paylaşım");
            AddProperty(type, TextBox("pageLanguage", "Dil kodu", "Örnek: tr veya en. Boşsa tr."), "seo", "SEO ve paylaşım");
            AddProperty(type, Toggle("hideHeader", "Üst menüyü gizle", "Landing page için."), "pageLayout", "Sayfa görünümü");
            AddProperty(type, Toggle("hideFooter", "Alt bilgiyi gizle", "Landing page için."), "pageLayout", "Sayfa görünümü");
            type.AllowedTemplates = [template];
            type.SetDefaultTemplate(template);
        }
        AddProperty(home, Toggle("builderEnabled", "Sayfa oluşturucuyu kullan", "Açıkken içerik Sayfa bölümleri listesinden gelir. Eski alanlar geçiş yedeği olarak korunur."), "builder", "Sayfa oluşturucu");
        AddProperty(home, TextBox("siteName", "Site adı", "Logo açıklaması ve varsayılan sayfa başlığı."), "brand", "Marka");
        AddProperty(home, Media("favicon", "Favicon", "Tarayıcı sekmesi simgesi.", mediaPicker), "brand", "Marka");
        AddProperty(home, UrlList("headerLinks", "Menü çağrı butonları", "Üst menünün sağındaki butonlar.", links), "nav", "Menü");
        foreach ((string alias, string label) in new[] { ("accentColor", "Vurgu rengi"), ("backgroundColor", "Sayfa arka planı"), ("headingColor", "Başlık rengi"), ("bodyColor", "Metin rengi"), ("surfaceColor", "Kart rengi") })
            AddProperty(home, Color(alias, label), "theme", "Tasarım");
        AddProperty(home, Media("bodyBackground", "Sayfa arka plan görseli", "Boşsa görsel kullanılmaz.", mediaPicker), "theme", "Tasarım");
        AddProperty(home, Media("footerBackground", "Alt bilgi arka planı", "Boşsa görsel kullanılmaz.", mediaPicker), "footer", "Alt bilgi");
        AddProperty(home, Choice("headingFont", "Başlık yazı tipi", fonts), "theme", "Tasarım");
        AddProperty(home, Choice("bodyFont", "Metin yazı tipi", fonts), "theme", "Tasarım");
        foreach ((string alias, string label, string range) in new[] { ("contentWidth", "İçerik genişliği (px)", "800–1800"), ("baseFontSize", "Metin boyutu (px)", "12–24"), ("headingSize", "Hero başlık boyutu (px)", "32–140"), ("cornerRadius", "Köşe yuvarlaklığı (px)", "0–48"), ("sectionGap", "Bölüm boşluğu (px)", "0–200"), ("logoWidth", "Logo genişliği (px)", "80–400") })
            AddProperty(home, NumericText(alias, label, range), "theme", "Tasarım");
        AddProperty(home, Toggle("disableMotion", "Animasyonları kapat", "Geçiş, karusel hareketi ve özel imleci kapatır."), "theme", "Tasarım");
        AddProperty(home, Toggle("disableCursor", "Özel imleci kapat", "Standart sistem imlecini kullanır."), "theme", "Tasarım");
        AddProperty(home, Toggle("disableSticky", "Sabit menüyü kapat", "Menü sayfayla birlikte kayar."), "nav", "Menü");
        AddProperty(home, Toggle("hideBackToTop", "Yukarı dön butonunu gizle", "Sağ alt köşedeki düğme."), "footer", "Alt bilgi");
        AddProperty(home, TextBox("socialLabel", "Sosyal medya etiketi", "Formların yanındaki başlık."), "footer", "Alt bilgi");
        AddProperty(home, TextBox("emailLabel", "E-posta etiketi", "Formların yanındaki başlık."), "footer", "Alt bilgi");
        AddProperty(home, RichText("footerText", "Ek alt bilgi metni", "Adres, kısa açıklama veya yasal metin.", richText), "footer", "Alt bilgi");
        AddProperty(home, UrlList("footerLinks", "Alt bilgi bağlantıları", "Gizlilik, KVKK ve diğer sayfalar.", links), "footer", "Alt bilgi");
        // Relax the old mandatory field: the builder may intentionally contain no hero.
        home.PropertyTypes.First(x => x.Alias == "heroLine1").Mandatory = false;
        foreach (IContentType type in new[] { home, page }) ConfigureFlexibleTabs(type);
        if (createPage)
        {
            var result = await _contentTypeService.CreateAsync(page, Constants.Security.SuperUserKey);
            if (!result.Success) throw new InvalidOperationException($"Could not create flexible page: {result.Result}");
            page = _contentTypeService.Get(FlexiblePageAlias)!;
        }
        page.AllowedContentTypes = [new ContentTypeSort(page.Key, 0, page.Alias)];
        var pageUpdate = await _contentTypeService.UpdateAsync(page, Constants.Security.SuperUserKey);
        if (!pageUpdate.Success) throw new InvalidOperationException($"Could not update flexible page: {pageUpdate.Result}");
        var allowed = home.AllowedContentTypes?.ToList() ?? [];
        if (!allowed.Any(x => x.Alias == page.Alias)) allowed.Add(new ContentTypeSort(page.Key, allowed.Count, page.Alias));
        home.AllowedContentTypes = allowed;
        var homeUpdate = await _contentTypeService.UpdateAsync(home, Constants.Security.SuperUserKey);
        if (!homeUpdate.Success) throw new InvalidOperationException($"Could not update flexible homepage: {homeUpdate.Result}");
        MigrateFlexibleContent(types, settings, media);
    }

    private IPropertyType Toggle(string alias, string name, string description)
        => Property(alias, name, description, Constants.DataTypes.Boolean, Constants.PropertyEditors.Aliases.Boolean, ValueStorageType.Integer, false, false);

    private IPropertyType Color(string alias, string name)
    {
        IPropertyType property = TextBox(alias, name, "HEX renk kodu: #64cdd2. Boşsa varsayılan rengi kullanır.");
        property.ValidationRegExp = "^#[0-9a-fA-F]{6}$";
        return property;
    }

    private IPropertyType NumericText(string alias, string name, string range)
    {
        IPropertyType property = TextBox(alias, name, $"{range} aralığında sayı. Boşsa varsayılan değer. Aralık dışı değerler güvenli sınıra çekilir.");
        property.ValidationRegExp = "^[0-9]{1,4}$";
        return property;
    }

    private IPropertyType Choice(string alias, string name, IDataType dataType)
        => Property(alias, name, "Boş bırakırsanız varsayılan görünüm kullanılır.", dataType, false, false);

    private async Task<IDataType> EnsureChoiceAsync(string name, params string[] values)
    {
        IDataType? existing = await _dataTypeService.GetAsync(name);
        if (existing is not null) return existing;
        var type = new DataType(RequireEditor(Constants.PropertyEditors.Aliases.DropDownListFlexible), _configurationSerializer)
        {
            Name = name, DatabaseType = ValueStorageType.Nvarchar, EditorUiAlias = "Umb.PropertyEditorUi.Dropdown",
            ConfigurationData = new Dictionary<string, object> { ["items"] = values, ["multiple"] = false },
        };
        var result = await _dataTypeService.CreateAsync(type, Constants.Security.SuperUserKey);
        if (!result.Success || result.Result is null) throw new InvalidOperationException($"Could not create {name}: {result.Status}");
        return result.Result;
    }

    private async Task<IDataType> EnsureBuilderDataTypeAsync(IEnumerable<IContentType> types, IContentType settings)
    {
        const string name = "Fuzul Sayfa oluşturucu";
        var blocks = types.Select(x => new Dictionary<string, object>
        {
            ["contentElementTypeKey"] = x.Key, ["settingsElementTypeKey"] = settings.Key,
            ["label"] = x.Name + " {=title}",
        }).ToArray();
        IDataType? existing = await _dataTypeService.GetAsync(name);
        if (existing is not null)
        {
            var config = new Dictionary<string, object>(existing.ConfigurationData) { ["blocks"] = blocks };
            existing.ConfigurationData = config;
            var updated = await _dataTypeService.UpdateAsync(existing, Constants.Security.SuperUserKey);
            if (!updated.Success || updated.Result is null) throw new InvalidOperationException($"Could not update page builder: {updated.Status}");
            return updated.Result;
        }
        var type = new DataType(RequireEditor(Constants.PropertyEditors.Aliases.BlockList), _configurationSerializer)
        {
            Name = name, DatabaseType = ValueStorageType.Ntext, EditorUiAlias = "Umb.PropertyEditorUi.BlockList",
            ConfigurationData = new Dictionary<string, object>
            {
                ["blocks"] = blocks,
                ["useLiveEditing"] = false,
            },
        };
        var result = await _dataTypeService.CreateAsync(type, Constants.Security.SuperUserKey);
        if (!result.Success || result.Result is null) throw new InvalidOperationException($"Could not create page builder: {result.Status}");
        return result.Result;
    }

    private static void ConfigureFlexibleTabs(IContentType type)
    {
        string[] order = ["builder", "brand", "nav", "theme", "footer", "seo", "pageLayout", "hero", "about", "sectors", "process", "committee", "apply", "contact"];
        foreach (PropertyGroup group in type.PropertyGroups)
        {
            group.Type = PropertyGroupType.Tab;
            var index = Array.IndexOf(order, group.Alias);
            group.SortOrder = index < 0 ? 99 : index;
            if (index >= 7) group.Name = "Eski · " + group.Name;
        }
    }

    private void MigrateFlexibleContent(IReadOnlyDictionary<string, IContentType> types, IContentType settingsType, MediaLibrary media)
    {
        if (_keyValueService.GetValue(FlexibleMigrationKey) == "1") return;
        foreach (IContent content in _contentService.GetRootContent().Where(x => x.ContentType.Alias == FuzulVenturesAliases.Home))
        {
            if (!IsEmpty(content, "pageSections")) continue;
            var publish = content.Published && !content.Edited;
            var layouts = new List<BlockListLayoutItem>();
            var data = new List<BlockItemData>();
            var settings = new List<BlockItemData>();
            var expose = new List<BlockItemVariation>();
            void Add(string alias, string anchor, IList<BlockPropertyValue> values)
            {
                Guid key = Guid.NewGuid();
                Guid settingsKey = Guid.NewGuid();
                layouts.Add(new BlockListLayoutItem(key, settingsKey));
                data.Add(new BlockItemData(key, types[alias].Key, alias) { Values = values });
                settings.Add(new BlockItemData(settingsKey, settingsType.Key, settingsType.Alias) { Values = BlockValues(("anchor", anchor), ("spacing", "[\"Normal\"]")) });
                expose.Add(new BlockItemVariation(key, null, null));
            }
            IList<BlockPropertyValue> Copy(params string[] aliases) => BlockValues(aliases.Select(x => (x, content.GetValue(x))).ToArray());
            var hero = Copy("heroLine1", "heroLine2", "heroLine3", "heroSubheading");
            hero.Add(new BlockPropertyValue { Alias = "links", Value = SerializeLinks([(content.GetValue<string>("heroButton") ?? "Yatırım Alanları", "#kurumsal", null)]) });
            hero.Add(new BlockPropertyValue { Alias = "image", Value = MediaPickerValue(media.Find("vent.jpg")) });
            Add("fuzulHeroSection", "hero", hero);
            Add("fuzulTextSection", "kurumsal", BlockValues(("title", content.GetValue("aboutTitle")), ("text", content.GetValue("aboutText"))));
            var sector = Copy("sectorsPrev", "sectorsNext", "sectorItems");
            sector.Add(new BlockPropertyValue { Alias = "layout", Value = "[\"Karusel\"]" });
            sector.Add(new BlockPropertyValue { Alias = "columns", Value = "[\"4\"]" });
            Add("fuzulSectorsSection", "alanlar", sector);
            Add("fuzulProcessSection", "surec", Copy("processTitle", "processEyebrow", "processSteps"));
            Add("fuzulCommitteeSection", "komite", Copy("committeeTitle", "committeeEyebrow", "committeeMembers"));
            Add("fuzulApplySection", "basvur", BlockValues(("title", content.GetValue("applyTitle")), ("text", content.GetValue("applyText"))));
            Add("fuzulContactSection", "contact", BlockValues(("title", content.GetValue("contactTitle")), ("text", content.GetValue("contactText"))));
            content.SetValue("pageSections", _jsonSerializer.Serialize(new BlockListValue(layouts) { ContentData = data, SettingsData = settings, Expose = expose }));
            content.SetValue("builderEnabled", true);
            if (IsEmpty(content, "siteName")) content.SetValue("siteName", "Fuzul Ventures");
            _contentService.Save(content);
            if (publish)
            {
                var result = _contentService.Publish(content, ["*"]);
                if (!result.Success) throw new InvalidOperationException("Flexible homepage could not be published; the migration will retry on next start.");
            }
            _logger.LogInformation("Migrated {Name} to flexible sections. Published: {Published}; existing drafts are preserved.", content.Name, publish);
        }
        _keyValueService.SetValue(FlexibleMigrationKey, "1");
    }
}
