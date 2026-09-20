namespace Umbraco.Cms.Web.UI.FuzulVentures;

internal static class FuzulVenturesSeedData
{
    public const string LogoUrl = "/fuzul/images/ventureslogo.svg";
    public const string Email = "girisim@fuzulventures.com";
    public const string Location = "Istanbul, TURKEY";

    public static readonly (string Label, string Url)[] Navigation =
    [
        ("Hakkımızda", "#kurumsal"),
        ("Yatırım Komitesi", "#komite"),
        ("Başvur", "#basvur"),
        ("Medya", "#medya"),
        ("İletişim", "#contact"),
        ("EN", "#"),
    ];

    public static readonly (string Label, string Url)[] Social =
    [
        ("Facebook", "https://www.facebook.com/fzlventures/"),
        ("Instagram", "https://www.instagram.com/fuzulventures/"),
        ("Twitter", "https://twitter.com/FuzulVentures"),
        ("Linkedin", "https://www.linkedin.com/company/fuzul-ventures/?viewAsMember=true"),
    ];

    public static readonly Sector[] Sectors =
    [
        new("Fintech", "Dijital dönüşümün öncüsü: Fintech yatırımıyla geleceği şekillendirin!", "/fuzul/images/h1.svg"),
        new("Proptech", "Geleceğin gayrimenkul endüstrisini teknolojik çözümlerle birleştiriken yanınızda olalım.", "/fuzul/images/h2.svg"),
        new("Gaming", "Potansiyeli yüksek oyun projelerine yatırım yapmak için buradayız.", "/fuzul/images/h3.svg"),
        new("MarTech", "Yeni nesil pazarlama teknolojilerine yatırım yapıyoruz.", "/fuzul/images/h4.svg"),
        new("Blokchain", "Blok Zincir teknolojilere yönelik girişimlere yatırım yapıyoruz.", "/fuzul/images/h5.svg"),
    ];

    public static readonly ProcessStep[] ProcessSteps =
    [
        new("01", "Başvuru", "Başvuru formunuz sisteme işlenerek Fuzul Ventures Yatırım komitesiyle paylaşılır.", "/fuzul/images/r1.svg"),
        new("02", "Değerlendirme", "Yatırım komitesi tarafından girişim incelenerek ön değerlendirme sürecine alınır.", "/fuzul/images/r2.svg"),
        new("03", "Komite Sunumu", "Ön değerlendirme sürecinde olumlu yaklaşılan girişimler komite sunumu için davet edilir.", "/fuzul/images/r3.svg"),
        new("04", "Onay", "Komite Sunumu beğenilen ve gelecek vadeden girişimler Komite Onayına sunulur.", "/fuzul/images/r6.svg"),
        new("05", "Yatırım", "Geleceğin lider şirketlerinin temelini oluşturacak finansal destek ile birlikte Fuzul Ventures’in deneyimi ve değerli network ağı her zaman yanınızda olur.", "/fuzul/images/r5.svg"),
    ];

    public static readonly CommitteeMember[] Members =
    [
        new(
            "Furkan Akbal",
            "Furkan Akbal",
            "1993 yılında dünyaya gelen Furkan Akbal 2016 yılında, İstanbul Bilgi Üniversitesi Hukuk Fakültesi’nden mezun oldu ve 2018 yılında İstanbul Ticaret Üniversitesi Sanayi Politikaları ve Teknoloji Yönetimi alanında yüksek lisans eğitimine başladı. Sivil Toplum Kuruluşlarının önemini bilen Akbal, 2016-2017 yıllarında TOBB İstanbul Genç Girişimciler Kurulu – Girişimci Kulüpler Platformunda görev almıştır. Halen Fuzul Holding bünyesinde faaliyet gösteren FuzulEv’in yönetim kurulu üyeliğinde bulunan Akbal, 2013 yılından bu yana MÜSİAD üyesidir ve 2014 – 2019 yılları arasında Genç MÜSİAD Kurul Üyesi olarak görev almıştır. DEİK Bahreyn-Türkiye İş Konseyi Başkan Yardımcılığı görevini halen yürütmekte olan Akbal ayrıca Galatasaray SK kongre üyesidir. 2019- 2023 yılları arasında 2 dönem Genç MÜSİAD Başkanı ve MÜSİAD Yönetim Kurulu Üyesi olarak görev yapan Furkan Akbal, iyi derecede İngilizce bilmektedir.",
            "/fuzul/images/farukakbal.png"),
        new(
            "Yusuf Akbal",
            "Reşit Yusuf Akbal",
            """
            20 Ekim 1992’de İstanbul Fatih’te doğan Yusuf Akbal, ASFA lisesinden mezun olduktan sonra lisans eğitimini İstanbul Şehir Üniversitesi İngilizce İşletme bölümünde tamamladı. Greenwich Üniversitesi’nde Stratejik Pazarlama bölümünde yüksek lisans eğitimi aldı.

            Kariyerine 2016 yılında, aile şirketi olan Fuzul Grup bünyesinde uluslararası gayrimenkul yatırım ve danışmanlık hizmetleri veren Nevita International şirketinde Genel Müdür olarak başladı.

            2014-2015 yıllarında Genç MÜSİAD bünyesinde gönüllü olarak sosyal faaliyetlerde bulunmak amacıyla Gıda ve Tarım Sektör Birimi’nde üye olarak görev aldı. 2015-2016 yıllarında Yurtdışı Teşkilatlanma Birimi’nde Başkan Yardımcılığını üstlendi. 2016-2019 yılları arasında ise aynı birimin Başkanlık görevini başarıyla yürütürken aynı zamanda Genç MÜSİAD’ın Yönetim Kurulu Üyesi olarak aktif rol aldı.

            GYODER bünyesinde Gayrimenkul İhracatçılar Komitesi’nde üye olan Akbal aynı zamanda 2019 Kasım ayından bu yana DEİK Libya ve Filistin İş Konseylerinde Yönetim Kurulu Üyesi olarak görev alıyor.

            İyi seviyede İngilizce ile temel düzeyde Arapça bilen Yusuf Akbal, evli ve iki çocuk babasıdır.
            """,
            "/fuzul/images/yusufakbal.png"),
        new(
            "Nizamettin Harputlu",
            "Nizamettin Harputlu",
            """
            Nizamettin Harputlu, ekip problemi nedeni ile 2 startup’ı denemesi olmuş ve yatırım aşamasında ciddi zorluklar aşmış, seri girişimcidir. Bu ekip problemini girişimcilik ekosistemindeki tüm startup’lar için çözülmesi gereken bir problem olarak görmüş ve bu problemi çözmek amacı ile StartupCentrum’u kurmuştur. StartupCentrum girişimleri, yatırımcıları, kuluçka merkezlerini ve girişimcilik ekosistemiyle ilgili yetenekleri bir araya getiren uluslararası dijital platformdur. 2000+ startup, 20.000’i aşkın kişi ve 200’yi aşkın fon, melek yatırımcı; Türkiye, Almanya, Fransa ve İngiltere gibi girişimcilik ekosistemlerinin raporları bulunmaktadır. Amerika Birleşik Devletleri merkezli StartupCentrum’un kurucu ortağıdır.

            Bunların yanı sıra Harputlu, Hazine ve Maliye Bakanlığı’na akredite melek yatırım ağı olarak onaylanan StartupCentrum Melek Yatırım Ağı ve SPK onaylı StartupCentrum Kitle Fonlama A.Ş.’nin yönetim kurulu başkanıdır. Birçok kuluçka merkezi, teknokent ve TEKMER’de mentorlük yapmakta ve eğitim vermektedir. Teknoloji firmaları ve uluslararası yatırımcılara danışmanlık yapmaktadır.

            Türkiye Teknoloji Vakfı’nın düzenlediği “fellow” programına seçilen girişimcilerden biri ve TÜSİAD Bu Gençlikte İŞ Var! Girişimcilik Programının Mezun Ağı Koordinatörüdür. Türkiye’nin ilk “blockchain fellow” programı olan Blockfellow’da Yönetim Kurulu Üyesi olarak yer almıştır. BKY2023000953 numaralı lisanslı melek yatırımcıdır.
            """,
            "/fuzul/images/nizamettinh.png"),
        new(
            "Emre Yıldız",
            "Emre Yıldız",
            """
            1987 doğumlu Yıldız, ortaokuldan bu yana tasarım ve üretkenlik konusunda hep faydalı olmaya çalıştı. Liseyi Derince Süper Lisesinde tamamlayan Yıldız, özel yetenek sınavı ile Kocaeli Üniversitesi Görsel İletişimi Tasarım’ı bölümünü kazandı.

            Üniversite birinci sınıfta iş hayatına da atıldı. 2007 yılında ünlü Türk müzik grubu MANGA için hazırladığı savaş karşıtı video klip ile Dream TV'de En Genç Video Yönetmeni olarak canlı yayına katıldı.

            Ajansının ismi EY08’in sebebini Emre Yıldız ilk faturasını 2008 yılında kesti olarak açıklayan Yıldız Kocaeli Üniversitesi'nden dereceyle mezun oldu. Aynı fakültede İletişim Bilimi Yüksek Lisansı programında devam etmektedir.

            2011-2013 yılları arasında Müstakil Sanayici ve İşadamları Derneği (MÜSİAD) Genç İl Başkanlığı görevini yürüten Yıldız, halen MÜSİAD, TOBB Genç Girişimciler Kurulu ve Türkiye Enerji Verimliliği Derneği üyesidir.

            2016 yılı itibariyle Bahçeşehir Üniversitesi MBA Programı'nda "Girişimcilik ve İnovasyon Yönetimi" dersleri vermeye başlayan Yıldız, bir yıl sonra "Hikaye" başlıklı TEDx konuşmasını yaptı.

            2018 yılından bu yana Türkiye'nin milli teknoloji alanındaki en önemli projelerinden biri olan Bilişim Vadisi’nde ‘’Girişimcilikten Sorumlu Genel Müdür Danışmanı’’ olarak görev yapan Yıldız 2020 yılı itibari ile Bilişim Vadisi'nde DİGİAGE (Dijital Animasyon & Oyun Merkezi) ve Erken Aşama Oyun Fonun da Direktör olarak görev yapmaktadır.

            Yıldız 2022 Yılı itibari ile Sistem Global icra kurulu üyesi olmuş ve grup içerisindeki teknoloji girişimlerinde yöneticilik ve YK üyeliği yapmaktadır. Sistem Global içerisinde kurulan ve girişimciler için hukuk, muhasebe, fon ve yatırımcıya erişim konularında hizmet veren sGLOBE’un kurucu ortağı ve fon yöneticisi olarak aktif görev almaktadır.

            İki başarız girişimi ve bir başarılı exit’i olan Yıldız şu an İstanbul, Londra ve Dubai’de faliyet gösteren perakende sektörü için oyun değiştirici girişim Octopus'un kurucusu ve CEO'su olarak çalışmaktadır.

            Evli ve Nil ve Mert’in babası olan Yıldız hayatı boyunca üretmek ve kendisi ile birlikte insanlığa faydalı olmak üzere çalışmaktadır
            """,
            "/fuzul/images/emre-yildiz.png"),
        new(
            "Hüseyin Nalbantoğlu",
            "Hüseyin Nalbantoğlu",
            """
            Hüseyin Nalbantoğlu 1994 yılında İstanbul’da doğdu. Lisans eğitimini Boğaziçi Üniversitesi İnşaat Mühendisliği bölümünde tamamladı.

            Lisans öğrenimi devam ederken ilk yazılım şirketini kurdu. 2 tur melek yatırım aldı, hisselerini stratejik yatırımcıya satıp ilk çıkışını yaptı. 4 melek yatırımı bulunan Nalbantoğlu, İTÜ Çekirdek ve Türk Telekom Pilot hızlandırma programlarında mentorluk yaptı. 2020 - 2022 yılları arasında 20+ startupa büyüme danışmanlığı verdi. Bugün yenilenebilir enerji alanında faaliyet gösteren bir mühendislik firmasının kurucu ortağıdır.

            İyi derecede İngilizce bilen Hüseyin Nalbantoğlu, evlidir.
            """,
            "/fuzul/images/11.jpg"),
    ];

    public sealed record Sector(string Title, string Text, string ImageUrl);

    public sealed record ProcessStep(string Number, string Title, string Text, string ImageUrl);

    public sealed record CommitteeMember(string Name, string ModalTitle, string Bio, string ImageUrl);
}
