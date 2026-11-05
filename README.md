# JerseyStore

ASP.NET Core MVC ile geliştirilen forma mağazası prototipi. Dapper ve SQL Server üzerinden ürün listeler, kişiselleştirilmiş forma siparişlerini kaydeder ve stok takibi yapar. Ürün kataloğu ve tarayıcıda tutulan sepet arayüzü içerir.

`pys_23` dalındaki ürün ve sepet çalışması bu sürüme dahil edilmiştir; sepet akışı korunup güvenli metin üretimi ve doğrulamayla düzenlenmiştir.

## Gereksinimler

- .NET 10 SDK
- SQL Server veya SQL Server Express
- İsteğe bağlı: Visual Studio / VS Code

## Yerel kurulum

1. Depoyu klonlayın ve kök klasöre geçin. Bu değişiklikler PR #1 içinde olduğu sürece `improve/project-quality` dalını seçin:

   ```sh
   git clone https://github.com/yagizodabas/JerseyStore.git
   cd JerseyStore
   git switch improve/project-quality
   ```

2. SQL Server üzerinde boş bir `JerseyStoreDb` veritabanı oluşturun.
3. Bu veritabanında `database/schema.sql` dosyasını çalıştırın. Bu dosya yalnızca yeni kurulum içindir; mevcut veritabanı için geçiş betiği değildir.
4. Bağlantıyı ayarlayın. Varsayılan ayar yerel Windows SQL Express içindir. Başka sunucuda ortam değişkeni kullanın:

   ```powershell
   $env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=JerseyStoreDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
   ```

   `TrustServerCertificate=True` yalnızca yerel geliştirme içindir. Üretimde doğrulanmış sunucu sertifikası kullanın. Parolalı bağlantıları Git'e eklemeyin.

5. Uygulamayı başlatın:

   ```sh
   dotnet restore JerseyStore.slnx --locked-mode
   dotnet build JerseyStore.slnx --configuration Release
   dotnet run --project JerseyStore --launch-profile https
   ```

Terminalde yazan adresi açın. HTTP profili `http://localhost:5062`, HTTPS profili `https://localhost:7145` kullanır. HTTPS sertifikası hazır değilse yerel geliştirme için `dotnet dev-certs https --trust` veya `dotnet run --project JerseyStore --launch-profile http` kullanın. Geliştirme ortamında `/swagger` etkileşimli API belgesidir. Ürün tablosu başlangıçta boştur; test ürünlerinizi veritabanına ekleyin.

## Sayfalar

| Yol | İçerik |
| --- | --- |
| `/` veya `/Products/Index` | Ürün kataloğu ve sepete ekleme |
| `/Orders/Cart` | Sepet, kişiselleştirme özeti ve teslimat formu |
| `/Home/Privacy` | Prototipin veri kullanımı açıklaması |

Fiyatlar mevcut sepet arayüzünün kullandığı Türk lirasıyla gösterilir. Sepetteki fiyatlar tahminidir; kesin tutarı sunucu hesaplar. Müşteri adı, adres ve telefon tarayıcı sepetine kaydedilmez. Sepet tarayıcı `localStorage` alanını kullanır; bozuk veya erişilemeyen depolama için hata gösterilir.

## API

| Yöntem | Yol | Açıklama |
| --- | --- | --- |
| GET | `/api/store/products` | Kimliğe göre sıralanmış ürünler |
| POST | `/api/store/order` | Sipariş oluşturur ve stoğu tek işlemde düşürür |

Örnek sipariş (ürün kimliğini kendi verinize göre değiştirin):

```json
{
  "customerName": "Örnek Müşteri",
  "address": "Örnek teslimat adresi",
  "phone": "+905551112233",
  "items": [
    { "productId": 1, "quantity": 1, "jerseyNumber": 10, "printName": "ÖRNEK" }
  ]
}
```

Adet 1–100, forma numarası 0–99 aralığındadır. Sepet en fazla 100 satır içerir. Fiyat ve toplam sunucuda hesaplanır. Sipariş zamanı UTC'dir. Geçersiz istek `400`, bulunamayan veya yetersiz stoklu ürün `409`, veritabanı hatası `503` döndürür. İşlem başarısızsa sipariş ve stok değişiklikleri geri alınır. Aynı ürünü içeren satırlar da sırayla stoktan düşülür.

## Yapı

- `JerseyStore/Controllers`: MVC ve HTTP API uçları
- `JerseyStore/Models`: Ürün ve doğrulanan sipariş istekleri
- `JerseyStore/Views`, `wwwroot`: Sunucu tarafından sunulan arayüz ve varlıklar
- [database/schema.sql](database/schema.sql): Yeni veritabanı şeması
- [docs/verification.md](docs/verification.md): SQL Server ile işlevsel kontrol adımları ve sonuçlar
- [docs/repository-review.md](docs/repository-review.md): Depo incelemesi ve kalan işler
- [CONTRIBUTING.md](CONTRIBUTING.md): Geliştirme ve katkı akışı
- `global.json`, `JerseyStore/packages.lock.json`: SDK seçimi ve kilitli bağımlılıklar
- `.github/workflows/build.yml`: Push ve PR için Release derlemesi

## Mevcut kapsam

Bu proje bir prototiptir. Ödeme, kullanıcı hesabı, yönetim paneli, kargo entegrasyonu ve sipariş durum sorgulama içermez. Üretim kullanımı öncesinde kimlik doğrulama/yetkilendirme, kötüye kullanım sınırlandırması, kişisel veri saklama politikası ve yinelenen siparişleri önleyen idempotency desteği tasarlanmalıdır. Yanıt alınamayan bir siparişi otomatik tekrar göndermek çift sipariş oluşturabilir.

Mevcut veritabanınız varsa yeni model sınırlarıyla sütun uzunluklarını karşılaştırın. Sipariş numarası artık `JS` + 32 karakterlik kimliktir; `OrderNumber` sütunu en az 34 karakter olmalıdır. Mevcut veriyi yedekleyip ayrı bir geçiş planı hazırlayın; şema dosyasını mevcut tablolar üzerinde çalıştırmayın.

## Otomatik doğrulama

Uygulama çalışırken PowerShell 7 ile:

```powershell
./scripts/smoke.ps1 -BaseUrl http://localhost:5062
```

Portu uygulamanın adresine göre değiştirin. Test, 11 geçersiz sipariş isteğinin veritabanına ulaşmadan reddedildiğini ve katalog HTML sayfasının sunulduğunu kontrol eder. GitHub Actions bu kontrolü derlemeden sonra çalıştırır. Gerçek sipariş ve eşzamanlılık testleri için `docs/verification.md` dosyasına bakın.
