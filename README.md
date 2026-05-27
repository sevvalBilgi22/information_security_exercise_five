🏛️ 1. Genel Mimari ve Mantık (The Big Picture)
Uygulama iki temel katmandan oluşuyor: Kimlik Doğrulama (Authentication) ve Veri Yönetimi (Data Management).

Veri Gizliliği İlkesi: Kullanıcının şifreleri (_data.txt) diskte asla açık metin (Plain Text) olarak durmaz.

Bellek Esaslı Çalışma: Program açık ve giriş yapılmışken veriler bilgisayarın geçici belleğinde (RAM) bir List<PasswordEntry> içinde çözülmüş olarak tutulur. Program kapandığı veya logout olunduğu an bu liste şifrelenerek diske yazılır ve RAM temizlenir.

🔐 2. Kullandığımız Algoritmalar ve Kod Karşılıkları
Projende iki farklı kriptografik yöntem kullandın. Bunların farkını bilmek en kritik noktadır.

A) SHA-256 (Kullanıcı Giriş Güvenliği)
Mantığı: Tek yönlü (One-Way) Hash fonksiyonudur. Yani hash'lenen bir veri asla geri çözülemez. users.txt dosyasında kullanıcı şifrelerini korumak için kullanılır.

Hocaya Açıklama: "Hocam, sisteme kayıt olunurken kullanıcının ana şifresini doğrudan kaydetmiyoruz. SHA-256 ile özetini (hash) alıyoruz. Giriş yaparken de girilen şifrenin hash'ini alıp bendekiyle karşılaştırıyorum. Böylece users.txt çalınsa bile kimsenin şifresi ifşa olmaz."

Kod Satırı: Form1.cs içindeki HashSHA256(string input) metodu.

B) AES - Advanced Encryption Standard (Dosya ve Şifre Güvenliği)
Mantığı: Simetrik (Symmetric) şifreleme algoritmasıdır. Yani veriyi şifrelemek için de, çözmek için de aynı anahtar (Key) kullanılır.

Hocaya Açıklama: "Hocam, dosyayı saklarken ve kişisel şifreleri şifrelerken AES-256 bit kullandık. Şifreleme anahtarı (Master Key) olarak kullanıcının giriş yaptığı kendi şifresini kullanıyoruz. Yani kullanıcı şifresini unuttuğu an verilerini kimse çözemez."

Kod Satırı: EncryptAES ve DecryptAES metotları.

📂 3. Adım Adım Akış (Workflow) ve Kod Satırları
Hoca sana "Hadi bana kod üzerinden göster, bu program nasıl çalışıyor?" dediğinde şu sırayla anlat:

Adım 1: Kullanıcı Girişi ve Otomatik Dosya Çözme (Mandatory + Optional 1 & 2)
Kullanıcı Login butonuna bastığında arka planda şunlar döner:

BtnLogin_Click metodu tetiklenir.

Kullanıcı adı ve şifre kontrol edildikten sonra LoadAndDecryptUserData(pass) çağrılır.

Eğer kullanıcının dosyası (kullaniciadi_data.txt) ilk defa oluşuyorsa, File.WriteAllText(CurrentUserFile, ""); satırı çalışır ve boş bir dosya yaratılır (Zorunlu İster - 3 Puan).

Eğer dosya varsa, DecryptAES(fileContent, masterPassword) satırı ile dosyanın şifresi çözülür, satırlar tek tek okunarak passwordList isimli bellekteki listeye yüklenir (İsteğe Bağlı İster - 1 Puan).

Adım 2: Yeni Şifre Ekleme ve AES ile Koruma (Mandatory - 0.5 Puan)
BtnAdd_Click metodu çalışır.

string encryptedPass = EncryptAES(txtAppPassword.Text, txtPassword.Text); satırı ile şifre, listeye eklenmeden hemen önce AES ile şifrelenir.

Diğer bilgiler (Title, URL, Notes) açık metin olarak kalırken, şifre veritabanına/listeye tamamen kilitli (Base64 formatında karmaşık karakterler olarak) girer.

Adım 3: Arama ve Güvenli Gösterim (Mandatory - 0.5 Puan & Optional 3)
TxtSearch_TextChanged metodu, kullanıcı arama kutusuna harf girdikçe listeyi anlık filtreler (passwordList.Where(...)).

Listeden bir öğe seçildiğinde LstEntries_SelectedIndexChanged tetiklenir.

Kritik Detay: Şifre hemen ekrana yazdırılmaz! lblSelectedDetails.Text güncellenirken şifre kısmına ***** yazılır. Gerçek şifre arka planda hiddenDecryptedPassword değişkeninde çözülmüş olarak bekler.

Kullanıcı Reveal Password butonuna basarsa BtnReveal_Click içindeki MessageBox.Show ile şifre güvenli bir şekilde gösterilir.

Adım 4: Güncelleme ve Silme (Mandatory - 0.5 + 0.5 Puan)
BtnUpdate_Click: Listeden seçilen verinin şifresi yeni girilen değerle tekrar EncryptAES edilerek güncellenir.

BtnDelete_Click: Seçilen veri passwordList.Remove(entry); satırı ile bellekten tamamen uçurulur.

Adım 5: Kapanışta/Çıkışta Otomatik Şifreleme (Mandatory + Optional 2)
Kullanıcı Secure Logout butonuna bastığında veya uygulamanın sağ üstündeki X butonuna basıp kapattığında Form1_FormClosing olayı tetiklenir.

Bu olay doğrudan SaveAndEncryptUserData() metodunu çağırır.

Metodun içindeki string encryptedContent = EncryptAES(sb.ToString(), txtPassword.Text); satırı bellekteki tüm listeyi birleştirip tek bir blok halinde AES ile şifreler.

File.WriteAllText(CurrentUserFile, encryptedContent); satırı ile şifreli blok dosyaya yazılır. Dosya artık tamamen kilitlidir.

🎯 Jüride Hayat Kurtaracak "Hoca Soru-Cevap" Tüyoları
Soru 1: Neden şifreli metinleri Base64 formatına çevirdin?

Cevap: "Hocam, AES şifreleme sonucunda bize okunamaz byte[] (bayt dizisi) verir. Bu ham baytları direkt .txt dosyasına düz metin gibi yazmaya çalışırsak bazı karakterler bozulabilir veya veri kaybı yaşanabilir. Convert.ToBase64String kullanarak bu baytları güvenli ve metin tabanlı (A-Z, a-z, 0-9) bir formata dönüştürüp dosyalarda hatasız sakladık."

Soru 2: PBKDF2 veya Argon2 yerine neden SHA-256 kullandın? (Opsiyonel İster Sorusu)

Cevap: "Hocam, ödev isterlerinde Bcrypt, PBKDF2 veya alternatif güvenli hashing yöntemleri öneriliyordu. .NET Framework mimarisinde harici bir kütüphane (NuGet) bağımlılığı yaratmamak ve uygulamanın taşınabilirliğini (portable) maksimumda tutmak adına System.Security.Cryptography kütüphanesinde yerleşik olarak bulunan, endüstri standardı SHA-256 algoritmasını tercih ettim."

Soru 3: Rastgele şifre üretirken neden Random sınıfını değil de RNGCryptoServiceProvider kullandın?

Cevap: "Hocam, C# içindeki standart Random sınıfı zamana dayalı (pseudo-random) çalışır ve tahmin edilebilir, yani kriptografik olarak güvenli değildir. Ben projemizde Kriptografik Güvenli Rastgele Sayı Üreticisi (CSPRNG) olan RNGCryptoServiceProvider sınıfını kullandım. Bu sayede üretilen şifrelerin kalıpları asla tahmin edilemez."
