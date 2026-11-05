# JerseyStore

ASP.NET Core MVC, Dapper ve SQL Server ile geliştirilen forma mağazası prototipi. Ürün listeleme, kişiselleştirilmiş forma siparişi oluşturma ve stok takibi için bir HTTP API içerir.

## Teknolojiler

- C# / .NET 10
- ASP.NET Core MVC
- SQL Server ve Microsoft.Data.SqlClient
- Dapper
- Swagger / Swashbuckle

## Mevcut özellikler

- Ürünlerin fiyat ve stok bilgileriyle listelenmesi
- Müşteri ve teslimat bilgileriyle sipariş oluşturulması
- Forma numarası ve baskı adı seçimi
- Sipariş toplamının sunucuda hesaplanması
- Sipariş, sipariş satırları ve stok değişikliklerinin veritabanı işlemi içinde kaydedilmesi
- Geliştirme ortamında Swagger API arayüzü

Ana daldaki web sayfası henüz varsayılan MVC şablonudur. Proje ödeme, kullanıcı hesabı, yönetim paneli ve kargo entegrasyonu içermez; üretime hazır bir e-ticaret sistemi değildir.

## Gereksinimler

- .NET 10 SDK
- SQL Server veya SQL Server Express
- İsteğe bağlı Visual Studio ya da VS Code

## Kurulum

```sh
git clone https://github.com/yagizodabas/JerseyStore.git
cd JerseyStore
dotnet restore JerseyStore.slnx
dotnet build JerseyStore.slnx
```

Varsayılan bağlantı yerel SQL Express üzerindeki `JerseyStoreDb` veritabanını kullanır. Başka bir sunucu için bağlantıyı ortam değişkeniyle ayarlayın:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost;Database=JerseyStoreDb;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
```

`TrustServerCertificate=True` yalnızca yerel geliştirme içindir. Üretimde doğrulanmış sertifika kullanın. Parola veya gerçek müşteri verilerini depoya eklemeyin.

**Veritabanı:** Bu ana dalda henüz şema veya migration dosyası bulunmuyor. Uygulama, aşağıdaki tabloların mevcut olmasını bekler; yalnızca uygulamayı başlatmak veritabanını oluşturmaz:

| Tablo | Kullanılan sütunlar |
| --- | --- |
| Products | Id, TeamName, Price, Stock |
| Orders | Id, OrderNumber, CustomerName, Address, Phone, OrderDate, TotalAmount |
| OrderItems | OrderId, ProductId, Quantity, UnitPrice, JerseyNumber, PrintName |

Veritabanı hazır olduğunda:

```sh
dotnet run --project JerseyStore --launch-profile https
```

Terminalde gösterilen adrese gidin. Geliştirme ortamında `/swagger` üzerinden API'yi inceleyebilirsiniz.

## API kullanımı

| Yöntem | Yol | Açıklama |
| --- | --- | --- |
| GET | `/api/store/products` | Ürünleri listeler |
| POST | `/api/store/order` | Sipariş oluşturur |

Örnek sipariş gövdesi; ürün kimliğini kendi veritabanınıza göre değiştirin:

```json
{
  "customerName": "Örnek Müşteri",
  "address": "Örnek teslimat adresi",
  "phone": "+905551112233",
  "items": [
    {
      "productId": 1,
      "quantity": 1,
      "jerseyNumber": 10,
      "printName": "ÖRNEK"
    }
  ]
}
```

## Proje yapısı

```text
JerseyStore.slnx
JerseyStore/
  Controllers/       MVC ve API uçları
  Models/            Ürün ve sipariş modelleri
  Views/             Razor sayfaları
  wwwroot/           CSS, JavaScript ve statik dosyalar
  Program.cs         Uygulama yapılandırması
  appsettings.json   Varsayılan yerel bağlantı ve log ayarları
```

## Hazırlanan iyileştirmeler

[PR #1](https://github.com/yagizodabas/JerseyStore/pull/1), sipariş doğrulaması, eşzamanlı stok güvenliği, mobil uyumlu ürün kataloğu, yeni veritabanı şeması, bağımlılık güvenlik düzeltmesi ve otomatik kontroller içerir. Bu değişiklikler henüz ana dala birleştirilmemiştir. Gerçek SQL Server üzerinde sipariş ve eşzamanlılık testleri ile tarayıcıda görsel doğrulama tamamlanmalıdır.
