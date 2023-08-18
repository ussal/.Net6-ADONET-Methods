# .Net6-ADONET-Methods
DataReader ile Veritabanı İşlemleri
EntityFramework yerine hızlıca ADO.NET kullanıp projeyi tamamlamak istediğinizde hazır yazılmış metotlar var mı diye düşünebilirsiniz. Bu düşünce boşa çıkmasın ve bende böyle bir düşünceye düşersem kaynağım bulunsun diye bu repo oluşturuldu.

Öncelikle yaptığımız işlem şu DataReader ile verileri Object, ObjectList olarak almak.

Ancak bu aşamada performansıda göz etmek.
Çünkü Connection Pooling diye bir detay var ve eğer connectionlar açık kalırsa bu başınıza iş çıkarabilir ve performansı düşüklüğü yaşatır.
Varsayılan olarak .Net6'da 100 Pool limiti vardır.
Bu demek oluyor ki 100 adet connection aynı anda açık kalabilir.

.Net6'da şöyle güzel bir özellik mevcut siz Connection String'de Pooling=False şeklinde özellikle belirtmediğinizde Pooling özelliği aktif olmaktadır.
Pooling özelliği, siz connection'u kapattığınızda bunu bir havuza atar ve tekrar aynı connection çağrıldığında o havuzdan kullanır bu sayede connection sürekli olarak açılıp kapanmamış olur.
Bizim kullandığımız metotlarda da bu özellik mevcuttur tabi eğer connection string'de Pooling=False yapmadıysanız.

Şunu da belirtmem gerekir ki DataReader'dan Object'e çevirmek için Dapper kütüphanesi kullanılmıştır.

### DataReader Methodumuz
```csharp
public SqlDataReader GetDataReader(string query, Dictionary<string, object>? parameters = null)
{
	SqlConnection con = new SqlConnection("Server=serverAddress;Database=dataBase;User Id=username;Password=password;");
	using (SqlCommand cmd = new SqlCommand(query, con))
	{
		if (parameters != null)
		{
			foreach (var parameter in parameters)
			{
				cmd.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
			}
		}
		con.Open();
		var reader = cmd.ExecuteReader(CommandBehavior.CloseConnection); //Reader kapandığında connectionu da kapatır.
		return reader;
	}
}
```
Gördüğünüz gibi connection string burada açılmış ama kapatılmamış. 
Eğer siz bu methodu kendiniz kullanıcak olursanız mutlaka reader'ı kullandıktan sonra kapatın ki connection'da havuza dönsün.
CommandBehavior.CloseConnection şeklinde belirttiğimiz yer reader kapatıldığında connection'un da havuza dönmesini sağlar.

Şimdi bu methodu kullanarak veritabanından çektiğimiz verileri objeye çevirelim.

### GetDataObject Methodu
```csharp
public T GetDataObject<T>(string query, Dictionary<string, object>? parameters = null) where T : class, new()
{
	using (var reader = GetDataReader(query, parameters))
	{
		var parser = reader.GetRowParser<T>(typeof(T)); //GetRowParser bir dapper methodudur paketi yüklemeniz gerek
		T myObject = new T();
		while (reader.Read())
		{
			myObject = parser(reader);
		}
		reader.Close();
		return myObject;
	}
}
```
### GetDataObject Örnek Kullanımı
```csharp
Dictionary<string, object> parameters = new(){{ "id", 1 }};
var myObject = GetDataObject<MyObject>("Select * from users where id=@id",parameters);
```

**Not:** Dönüştürmek istediğimiz obje ile veritabanından dönen sütun adının aynı olması gerekir ki çevirmeyi doğru yapabilsin.

### GetDataList Methodu
```csharp
public List<T> GetDataList<T>(string query, Dictionary<string, object>? parameters = null) where T : class, new()
{
	using (var reader = GetDataReader(query, parameters))
	{
		var parser = reader.GetRowParser<T>(typeof(T)); //GetRowParser bir dapper methodudur paketi yüklemeniz gerek
		List<T> myList = new List<T>();
		while (reader.Read())
		{
			myList.Add(parser(reader));
		}
		reader.Close();
		return myList;
	}
}
```
GetDataObject metohudunun liste dönen halidir. Methodu kullanırken parametre zorunlu değildir.

### GetDataList Örnek Kullanımı
```csharp
var myObjects = GetDataList<MyObject>("Select * from users");
```

Siz parametrede verebilirsiniz. GetDataObject kullanımında parametreli kullanım olduğu için burada kullanılmadan da olduğunu göstermek için böyle kullanıldı.

Birde obje değilde sadece string bir değer almak isterseniz diye şöyle de bir method yazdık.

### GetDataString Methodu
```csharp
public string GetDataString(string query, Dictionary<string, object>? parameters = null)
{
	using (var reader = GetDataReader(query, parameters))
	{
		string dataString = String.Empty;
		while (reader.Read())
		{
			dataString = reader.IsDBNull(0) ? "" : reader.GetString(0);
		}
		reader.Close();
		return dataString;
	}
}
```
