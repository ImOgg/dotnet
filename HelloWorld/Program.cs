// See https://aka.ms/new-console-template for more information
//參考學習 https://ithelp.ithome.com.tw/users/20178767/ironman/8726
// 到12 章為止，我學不下去了，Day 12 也還沒學完
using System.Diagnostics.CodeAnalysis;

// ============================
// 1. Hello World - 基本輸出
// ============================
// Console.WriteLine("Hello, World!");

// ============================
// 2. 讀取使用者輸入 + 日期格式化
// ============================
// Console.WriteLine("What is your name?");
// var name = Console.ReadLine();
// var currentDate = DateTime.Now;
// Console.WriteLine($"{Environment.NewLine}Hello, {name}, on {currentDate:d} at {currentDate:t}!");
// Console.Write($"{Environment.NewLine}Press Enter to exit...");
// Console.Read();

// ============================
// 3. 讀取兩個數字並相加
// ============================
// Console.WriteLine("Enter 2 number:");
// var input1 = Console.ReadLine();
// var input2 = Console.ReadLine();
// int num1 = int.Parse(input1);
// int num2 = int.Parse(input2);
// Console.Write($"{Environment.NewLine}Two number total is {num1+num2}");
// Console.Write($"{Environment.NewLine}Press Enter to exit...");
// Console.Read();

// ============================
// 4. 字串（string）基本操作
// ============================
// string firstFriend = "Maria";
// string secondFriend = "Sage";
// Console.WriteLine($"My friends are {firstFriend} and {secondFriend}");

// string sayHello = "Hello World!";
// Console.WriteLine(sayHello);
// sayHello = sayHello.Replace("Hello", "Greetings");   // Replace：取代字串
// Console.WriteLine(sayHello);

// string songLyrics = "You say goodbye, and I say hello";
// Console.WriteLine(songLyrics.Contains("goodbye"));   // Contains：是否包含某字串
// Console.WriteLine(songLyrics.Contains("greetings"));

// ============================
// 5. 整數（int）四則運算
// ============================
// int a = 18;
// int b = 6;
// int c = a + b;
// Console.WriteLine(c);

// int a = 5;
// int b = 4;
// int c = 2;
// int d = a + b * c;   // 先乘除後加減
// Console.WriteLine(d);

// d = (a + b) - 6 * c + (12 * 4) / 3 + 12;
// Console.WriteLine(d);

// int a = 7;
// int b = 4;
// int c = 3;
// int d = (a + b) / c;   // 整數除法：結果無小數
// int e = (a + b) % c;   // % 取餘數
// Console.WriteLine($"quotient: {d}");
// Console.WriteLine($"remainder: {e}");

// ============================
// 6. 浮點數（double / decimal）
// ============================
// double a = 5;
// double b = 4;
// double c = 2;
// double d = (a + b) / c;
// Console.WriteLine(d);

// double a = 19;
// double b = 23;
// double c = 8;
// double d = (a + b) / c;
// Console.WriteLine(d);

// double a = 1.0;
// double b = 3.0;
// Console.WriteLine(a / b);   // double：精度較低，適合科學計算

// decimal c = 1.0M;           // M 後綴 = decimal 型別
// decimal d = 3.0M;
// Console.WriteLine(c / d);   // decimal：精度較高，適合金融計算

// ============================
// 7. Tuple（具名元組）
// ============================
// var pt = (X: 1, Y: 2);   // Tuple：輕量的多值容器，不需要定義 class

// var slope = (double)pt.Y / (double)pt.X;
// Console.WriteLine($"A line from the origin to the point {pt} has a slope of {slope}.");

// pt.X = pt.X + 5;   // Tuple 的屬性可以修改（與 record 不同）
// Console.WriteLine($"The point is now at {pt}.");

// var pt2 = pt with { Y = 10 };
// Console.WriteLine($"The point 'pt2' is at {pt2}.");

// var namedData = (Name: "Morning observation", Temp: 17, Wind: 4);
// var person = (FirstName: "Lee", LastName: "Jessica");
// var order = (Product: "guitar picks", style: "triangle", quantity: 500, UnitPrice: 0.10m);
// Console.WriteLine($"My name is {person.LastName} {person.FirstName}. {namedData.Name} temp is {namedData.Temp} and wind is {namedData.Wind}.");

// ============================
// 8. record + with 語法
// ============================
// public class Program
// {
//     // 方法（method）不能獨立存在，必須放在類別（class）裡面。
//     public static void Main()
//     {
//         Point pt = new Point(1, 1);
//         var pt2 = pt with { Y = 10 };
//         Console.WriteLine($"The two points are {pt} and {pt2}");
//     }
// }

// record 是 C# 9 引入的型別，專門用來表示「不可變的資料」
// 語法：public record 名稱(屬性1, 屬性2, ...)
// 特性：
//   1. 自動產生建構子、getter、ToString()、Equals() 等方法
//   2. 不可變（immutable）：建立後屬性值無法直接修改
//   3. with 語法：可以複製一個 record，並只改變指定的屬性
//      例：var pt2 = pt with { Y = 10 };  → 複製 pt，但把 Y 改成 10
//   4. 與 class 的差異：record 比較值（value equality），class 比較參考（reference equality）
// public record Point(int X, int Y);

// int a = 5;
// int b = 3;
// if (a + b > 10)
//     Console.WriteLine("The answer is greater than 10");
// else
//     Console.WriteLine("The answer is not greater than 10");


// char grade = 'B';

// switch (grade)
// {
//     case 'A':
//         Console.WriteLine("優秀");
//         break;
//     case 'B':
//         Console.WriteLine("良好");
//         break;
//     case 'C':
//         Console.WriteLine("及格");
//         break;
//     default:
//         Console.WriteLine("需要加油");
//         break;
// }

// for (int counter = 0; counter < 10; counter++)
// {
//     Console.WriteLine($"Hello World! The counter is {counter}");
// }

// for (int row = 1; row < 11; row++)
// {
//     for (char column = 'a'; column < 'k'; column++)
//     {
//         Console.WriteLine($"The cell is ({row}, {column})");
//     }
// }

// int counter = 0;
// while (counter < 10)
// {
//     Console.WriteLine($"Hello World! The counter is {counter}");
//     counter++;
// }

// int counter = 0;
// do
// {
//     Console.WriteLine($"Hello World! The counter is {counter}");
//     counter++;
// } while (counter < 10);

// string[] fruits = { "蘋果", "香蕉", "葡萄" ,"櫻桃"};

// foreach (string fruit in fruits)
// {
//     Console.WriteLine(fruit);
// }

// DisplayWeatherReport(15.0);  // Output: Cold.
// DisplayWeatherReport(24.0);  // Output: Perfect!

// void DisplayWeatherReport(double tempInCelsius)
// {
//     if (tempInCelsius < 20.0)
//     {
//         Console.WriteLine("Cold.");
//     }
//     else
//     {
//         Console.WriteLine("Perfect!");
//     }
// }

// DisplayMeasurement(45);  // 輸出: The measurement value is 45
// DisplayMeasurement(-3);  // 輸出: Warning: not acceptable value! The measurement value is -3

// void DisplayMeasurement(double value)
// {
//     if (value < 0 || value > 100)
//     {
//         Console.Write("Warning: not acceptable value! ");
//     }

//     Console.WriteLine($"The measurement value is {value}");
// }

// DisplayCharacter('f');  // 輸出: A lowercase letter: f
// DisplayCharacter('R');  // 輸出: An uppercase letter: R
// DisplayCharacter('8');  // 輸出: A digit: 8
// DisplayCharacter(',');  // 輸出: Not alphanumeric character: ,

// void DisplayCharacter(char ch)
// {
//     if (char.IsUpper(ch))
//     {
//         Console.WriteLine($"An uppercase letter: {ch}");
//     }
//     else if (char.IsLower(ch))
//     {
//         Console.WriteLine($"A lowercase letter: {ch}");
//     }
//     else if (char.IsDigit(ch))
//     {
//         Console.WriteLine($"A digit: {ch}");
//     }
//     else
//     {
//         Console.WriteLine($"Not alphanumeric character: {ch}");
//     }
// }

// ============================
// 10. abstract 修飾詞
// ============================
// abstract 用於類別或成員（方法、屬性、索引子、事件），
// 表示該成員沒有完整實作，必須由衍生類別（derived class）來實作。
//
// 抽象類別（abstract class）：
//   - 不能被直接建立物件（new）
//   - 可包含抽象成員與已實作成員
//   - 常作為「基底類別」(blueprint) 提供共用功能
//   - 可以有欄位、已實作的方法、屬性、建構子
//   - 可以有 abstract 方法或屬性，衍生類別必須 override
//   - 不能與 sealed 同時使用（sealed 代表不能被繼承）

// namespace MotorCycleExample
// {
//     abstract class Motorcycle
//     {
//         // 任何人都能呼叫
//         public void StartEngine() { /* 方法內容 */ }

//         // 只有衍生類別 (繼承者) 可以呼叫
//         protected void AddGas(int gallons) { /* 方法內容 */ }

//         // 衍生類別可以覆寫 (override) 這個方法
//         public virtual int Drive(int miles, int speed)
//         {
//             /* 方法內容 */
//             return 1;
//         }

//         // 方法多載 (overloading)：同名方法但參數不同
//         public virtual int Drive(TimeSpan time, int speed)
//         {
//             /* 方法內容 */
//             return 0;
//         }

//         // 抽象方法：強制衍生類別必須實作
//         public abstract double GetTopSpeed();
//     }
// }

// public static class SquareExample
// {
//     // 程式進入點
//     public static void Main()
//     {
//         // 用變數呼叫 Square：計算 4 的平方
//         int num = 4;
//         int productA = Square(num);       // productA = 16
//         Console.WriteLine($"The square of {num} is {productA}.");
//         // 用整數字面值呼叫 Square：計算 12 的平方
//         int productB = Square(12);        // productB = 144
//         Console.WriteLine($"The square of 12 is {productB}.");
//         // 用運算式呼叫 Square：計算 (16 * 3) = 48 的平方
//         int productC = Square(productA * 3);  // productC = 2304
//         Console.WriteLine($"The square of {productA} * 3 is {productC}.");
//         // ⚠️ 注意：這裡只有計算，沒有 Console.WriteLine()
//         //    所以執行後 terminal 不會有任何輸出！
//     }

//     // 計算整數的平方（i * i）並回傳結果
//     static int Square(int i)
//     {
//         int input = i;
//         return input * input;
//     }
// }

// namespace MotorCycleExample
// {
//     abstract class Motorcycle
//     {
//         // Anyone can call this.
//         public void StartEngine() {/* Method statements here */ }

//         // Only derived classes can call this.
//         public void AddGas(int gallons) { /* Method statements here */ }

//         // Derived classes can override the base class implementation.
//         public virtual int Drive(int miles, int speed) { /* Method statements here */ return 1; }

//         // Derived classes can override the base class implementation.
//         public virtual int Drive(TimeSpan time, int speed) { /* Method statements here */ return 0; }

//         // Derived classes must implement this.
//         public abstract double GetTopSpeed();
//     }
//     class TestMotorcycle : Motorcycle
//     {
//         public override double GetTopSpeed() => 108.4;

//         static void Main()
//         {
//             var moto = new TestMotorcycle();

//             moto.StartEngine();
//             moto.AddGas(15);
//             _ = moto.Drive(5, 20);
//             double speed = moto.GetTopSpeed();
//             Console.WriteLine($"My top speed is {speed}");
//         }
//     }
// }

// public class Person
// {
//     public string FirstName = default!;
// }

// public static class ClassTypeExample
// {
//     public static void Main()
//     {
//         Person p1 = new() { FirstName = "John" };
//         Person p2 = new() { FirstName = "John" };
//         // Equals 是比較兩個物件是否相等的方法。
//         Console.WriteLine($"p1 = p2: {p1.Equals(p2)}");
//     }
// }
// 輸出：p1 = p2: False

// namespace methods;

// public class Person
// {
//     public string FirstName = default!;

//     public override bool Equals(object? obj) =>
//         obj is Person p2 &&
//         FirstName.Equals(p2.FirstName);

//     public override int GetHashCode() => FirstName.GetHashCode();
// }

// public static class Example
// {
//     public static void Main()
//     {
//         Person p1 = new() { FirstName = "John" };
//         Person p2 = new() { FirstName = "John" };
//         Console.WriteLine($"p1 = p2: {p1.Equals(p2)}");
//     }
// }
// 輸出：p1 = p2: True

// Value Type傳值
// 當把值型別 (如 int, double, struct) 傳遞給方法時，
// 會傳遞「值的複本」，Method內對參數的修改，不會影響到原本變數。
// public static class ByValueExample
// {
//     public static void Main()
//     {
//         var value = 20;
//         Console.WriteLine("In Main, value = {0}", value);
//         ModifyValue(value);
//         Console.WriteLine("Back in Main, value = {0}", value);
//     }

//     static void ModifyValue(int i)
//     {
//         i = 30;
//         Console.WriteLine("In ModifyValue, parameter value = {0}", i);
//     }
// }

// ref 傳參考（Reference Type傳址）
// 傳遞的是變數的「記憶體位址」，方法內的修改會直接影響原本的變數。
// 注意：呼叫端和方法定義都必須加上 ref 關鍵字。
// public static class ByRefExample
// {
//     public static void Main()
//     {
//         var value = 20;
//         Console.WriteLine("In Main, value = {0}", value);           // 輸出：20
//         ModifyValue(ref value);                                      // 傳址：value 的位址傳進去
//         Console.WriteLine("Back in Main, value = {0}", value);      // 輸出：30（原變數被改變）
//     }

//     // ref int i：接收的是 value 的記憶體位址，修改 i 等於修改 value
//     private static void ModifyValue(ref int i)
//     {
//         i = 30;
//         Console.WriteLine("In ModifyValue, parameter value = {0}", i); // 輸出：30
//     }
// }

// ref 的實際應用：用 ref 交換兩個變數的值
// public static class RefSwapExample
// {
//     static void Main()
//     {
//         int i = 2, j = 3;
//         Console.WriteLine($"i = {i}  j = {j}");   // 輸出：i = 2  j = 3

//         Swap(ref i, ref j);   // 傳入 i 和 j 的記憶體位址，讓 Swap 直接互換

//         Console.WriteLine($"i = {i}  j = {j}");   // 輸出：i = 3  j = 2（已互換）
//     }

//     // 用 ref 接收兩個變數的位址，然後用 Tuple 解構語法一行完成互換
//     // (y, x) = (x, y)：等號右邊先讀取 x、y 的值，再同時賦值給左邊的 y、x
//     static void Swap(ref int x, ref int y) =>
//         (y, x) = (x, y);
// }

// params：讓方法可以接受「數量不固定」的參數
// 呼叫端可以傳入陣列、多個引數、null，或什麼都不傳
// static class ParamsExample
// {
//     static void Main()
//     {
//         // 用集合運算式（collection expression）傳入陣列
//         string fromArray = GetVowels(["apple", "banana", "pear"]);
//         Console.WriteLine($"Vowels from collection expression: '{fromArray}'");  // 輸出：aaeaa

//         // 直接傳多個字串引數（params 自動打包成集合）
//         string fromMultipleArguments = GetVowels("apple", "banana", "pear");
//         Console.WriteLine($"Vowels from multiple arguments: '{fromMultipleArguments}'");  // 輸出：aaeaa

//         // 傳 null：input == null，回傳空字串
//         string fromNull = GetVowels(null);
//         Console.WriteLine($"Vowels from null: '{fromNull}'");  // 輸出：（空）

//         // 什麼都不傳：input.Any() == false，回傳空字串
//         string fromNoValue = GetVowels();
//         Console.WriteLine($"Vowels from no value: '{fromNoValue}'");  // 輸出：（空）
//     }

//     // params IEnumerable<string>?：接受任意數量的字串集合，可為 null
//     // 回傳所有單字中的母音字母（不分大小寫）串接成一個字串
//     static string GetVowels(params IEnumerable<string>? input)
//     {
//         // 若 input 為 null 或沒有任何元素，回傳空字串
//         if (input == null || !input.Any())
//         {
//             return string.Empty;
//         }

//         char[] vowels = ['A', 'E', 'I', 'O', 'U'];  // 母音清單（大寫）
//         return string.Concat(
//             input.SelectMany(                          // 展開每個單字
//                 word => word.Where(letter => vowels.Contains(char.ToUpper(letter)))));  // 篩選母音字母
//     }
// }

// // ============================
// // 9. 類別、物件與屬性（Class, Object & Property）
// // ============================
// // Top-level statements 必須放在型別宣告之前

// // 用有參數建構子建立物件（FirstName 由建構子設定，其餘用初始化語法補上）
// var p1 = new Person("Grace") { LastName = "Hopper", Age = 85 };
// // 用無參數建構子建立物件（required 屬性全部用初始化語法設定）
// var p2 = new Person() { FirstName = "Ada", LastName = "Lovelace", Age = 36 };

// p1.Nickname = "  Admiral Grace  ";  // setter 會自動 Trim() 去除空白
// p1.SetEmail("grace@navy.mil");      // Email 是 private set，只能透過方法修改
// p1.Login();
// p1.Login();

// Console.WriteLine("--- 基本屬性 ---");
// Console.WriteLine(p1);                                  // 輸出：[Person] Grace Hopper, Age: 85
// Console.WriteLine(p2);                                  // 輸出：[Person] Ada Lovelace, Age: 36

// Console.WriteLine("\n--- 自訂 setter / Trim ---");
// Console.WriteLine($"Nickname: '{p1.Nickname}'");        // 輸出：'Admiral Grace'（已去除空白）

// Console.WriteLine("\n--- private set ---");
// Console.WriteLine($"Email: {p1.Email}");                // 輸出：grace@navy.mil
// // p1.Email = "xxx";                                    // ❌ 編譯錯誤：Email 是 private set

// Console.WriteLine("\n--- 唯讀屬性（Read-only）---");
// Console.WriteLine($"CreatedAt: {p1.CreatedAt}");        // 只有 p1 有值（用有參數建構子建立）
// Console.WriteLine($"CreatedAt: {p2.CreatedAt}");        // 輸出：01/01/0001（DateTime 預設值，無參數建構子未設定）

// Console.WriteLine("\n--- Lazy Loading FullName 快取 ---");
// Console.WriteLine($"FullName: {p1.FullName}");          // 輸出：Grace Hopper（第一次計算並快取）
// p1.LastName = "Murray";                                 // 修改 LastName → 清空快取
// Console.WriteLine($"FullName: {p1.FullName}");          // 輸出：Grace Murray（重新計算）

// Console.WriteLine("\n--- 方法 ---");
// Console.WriteLine($"LoginCount: {p1.GetLoginCount()}"); // 輸出：2

// Console.Write($"\nPress Enter to exit...");
// Console.Read();

// // 型別宣告放在 top-level statements 之後
// public class Person
// {
//     // 私有欄位（Field）：不對外公開，搭配屬性或方法使用
//     private int _loginCount = 0;
//     private string? _nickname;
//     private string? _lastName;
//     private string? _fullName;  // FullName 的快取欄位

//     // 無參數建構子（Parameterless Constructor）
//     public Person() { }

//     // 有參數建構子 + [SetsRequiredMembers]：
//     // 告訴編譯器此建構子已滿足所有 required 屬性，呼叫端不需再補給
//     [SetsRequiredMembers]
//     public Person(string firstName)
//     {
//         FirstName = firstName;
//         CreatedAt = DateTime.Now;  // 唯讀屬性只能在建構子裡賦值
//     }

//     // required + init：建立時必須給值，建立後不可修改（唯讀）
//     public required string FirstName { get; init; }

//     // required + 自訂 setter：修改 LastName 時清空 FullName 快取
//     public required string LastName
//     {
//         get => _lastName;
//         set
//         {
//             _lastName = value;
//             _fullName = null;  // 清空快取，讓 FullName 下次重新計算
//         }
//     }

//     // 預設值屬性 + private set：外部只能讀，只能透過類別內部方法修改
//     public string Email { get; private set; } = string.Empty;
//     public void SetEmail(string email) => Email = email;  // 透過方法修改 Email

//     // 自訂 setter：寫入時自動去除前後空白
//     public string? Nickname
//     {
//         get => _nickname;
//         set => _nickname = value?.Trim();
//     }

//     // 唯讀屬性（Read-only）：只有 get，只能在建構子設值，外部完全不能修改
//     public DateTime CreatedAt { get; }

//     // Lazy Loading 計算屬性：第一次呼叫才計算並快取，LastName 改變時會重新計算
//     public string FullName
//     {
//         get
//         {
//             // ??= 複合賦值：若 _fullName 是 null 才計算並賦值，等同 if (_fullName is null) _fullName = ...
//             _fullName ??= $"{FirstName} {LastName}";
//             return _fullName;
//         }
//     }

//     // 一般自動屬性
//     public int Age { get; set; }

//     // 方法：操作私有欄位
//     public void Login() => _loginCount++;
//     public int GetLoginCount() => _loginCount;

//     // 覆寫 ToString()：讓 Console.WriteLine(物件) 有意義的輸出
//     public override string ToString() =>
//         $"[Person] {FullName}, Age: {Age}";
// }

// // 建立 WorkItem 物件
// WorkItem item = new WorkItem("Fix Bugs",
//                             "Fix all bugs in my code branch",
//                             new TimeSpan(3, 4, 0, 0));

// // 建立 ChangeRequest 物件
// ChangeRequest change = new ChangeRequest("Change Base Class Design",
//                                         "Add members to the class",
//                                         new TimeSpan(4, 0, 0),
//                                         1);

// // 使用 WorkItem 的 ToString()
// Console.WriteLine(item.ToString());

// // 使用繼承自 WorkItem 的 Update()
// change.Update("Change the Design of the Base Class",
//     new TimeSpan(4, 0, 0));

// // ChangeRequest 繼承了 WorkItem 的 ToString()
// Console.WriteLine(change.ToString());

// // WorkItem 類別，繼承自 Object（所有 C# 類別的根類別）
// public class WorkItem
// {
//     // 靜態欄位：所有 WorkItem 物件共用同一個 currentID 計數器
//     // static 表示屬於「類別本身」，不屬於任何單一物件
//     private static int currentID;

//     // protected 屬性：本類別和子類別可存取，外部無法直接讀寫
//     protected int ID { get; set; }
//     protected string Title { get; set; }
//     protected string Description { get; set; }
//     protected TimeSpan jobLength { get; set; }

//     // 預設建構子（無參數）
//     // 用途：先建立物件、之後再填資料；或供子類別繼承時呼叫
//     public WorkItem()
//     {
//         ID = 0;
//         Title = "Default title";
//         Description = "Default description.";
//         jobLength = new TimeSpan();
//     }

//     // 帶參數的建構子
//     // 用途：一次建立並填入所有資料，ID 自動遞增
//     public WorkItem(string title, string desc, TimeSpan joblen)
//     {
//         this.ID = GetNextID();   // 取得下一個唯一 ID
//         this.Title = title;
//         this.Description = desc;
//         this.jobLength = joblen;
//     }

//     // 靜態建構子：程式啟動時只執行一次，用來初始化靜態欄位
//     // 不能有參數、不能有存取修飾詞（public/private）
//     static WorkItem() => currentID = 0;

//     // 產生下一個 ID：每次呼叫都讓 currentID +1 再回傳
//     // ++currentID 是前綴遞增（先加後回傳），確保 ID 從 1 開始
//     protected int GetNextID() => ++currentID;

//     // 更新工作項目的標題和預估時間
//     public void Update(string title, TimeSpan joblen)
//     {
//         this.Title = title;
//         this.jobLength = joblen;
//     }

//     // 覆寫 Object.ToString()：讓 Console.WriteLine(物件) 輸出有意義的字串
//     // 格式："ID - Title"，例如："1 - Fix Bugs"
//     public override string ToString() =>
//         $"{this.ID} - {this.Title}";
// }

// // ChangeRequest 繼承 WorkItem
// // 代表「變更請求」，在原有工作項目基礎上多記錄一個來源 ID
// public class ChangeRequest : WorkItem
// {
//     // 記錄這個變更是針對哪一個原始 WorkItem（用 ID 關聯）
//     protected int originalItemID { get; set; }

//     // 預設建構子：空實作，自動呼叫父類別 WorkItem() 預設建構子
//     // 繼承時若不寫這個，某些框架（如序列化）會找不到無參數建構子而報錯
//     public ChangeRequest() { }

//     // 帶參數的建構子：建立完整的變更請求
//     // 因為父類別欄位是 protected，子類別可以直接存取並賦值
//     public ChangeRequest(string title, string desc, TimeSpan jobLen,
//                          int originalID)
//     {
//         this.ID = GetNextID();            // 繼承自 WorkItem 的方法，取得唯一 ID
//         this.Title = title;
//         this.Description = desc;
//         this.jobLength = jobLen;
//         this.originalItemID = originalID; // 記錄來源 WorkItem 的 ID
//     }
// }

// ============================
// 介面（Interface）- 飲料店情境
// ============================

// 情境說明：
// 飲料店店員同時面對「顧客」和「廚房」兩種角色。
// 顧客可以直接叫店員報菜單、收款；
// 但下訂單給廚房、通知出餐這些內部操作，
// 不應該讓顧客隨意呼叫，所以用「明確介面實作」隱藏起來。

// // 建立店員物件，名字傳入建構子
// DrinkShopStaff staff = new DrinkShopStaff("小美");

// // ✅ 顧客端：ICustomerFacing 的方法是 public，可以直接透過物件呼叫
// staff.ShowMenu();
// staff.TakePayment(120);

// // ❌ 廚房功能不能直接呼叫（明確介面實作不會出現在類別的公開成員上）
// // staff.TakeOrder("珍珠奶茶", 2);  → 編譯錯誤

// // ✅ 廚房端：將 staff 強制轉型成 IKitchenFacing，才能呼叫廚房方法
// // 這就是「明確介面實作」的使用方式：(介面型別)物件.方法()
// ((IKitchenFacing)staff).TakeOrder("珍珠奶茶", 2);
// ((IKitchenFacing)staff).NotifyKitchen();

// // ──────────────────────────────
// // IEquatable<T>：泛型介面，讓類別定義自己的「相等」規則
// // 預設 class 是比較記憶體位址（參考相等），不是比較內容
// // 實作 IEquatable<T> 後，可以按照欄位值來判斷兩個物件是否「一樣」
// Drink d1 = new Drink { Name = "珍珠奶茶", Size = "大杯" };
// Drink d2 = new Drink { Name = "珍珠奶茶", Size = "大杯" };  // 內容相同
// Drink d3 = new Drink { Name = "綠茶",     Size = "中杯" };  // 內容不同
// Console.WriteLine($"\nd1 == d2：{d1.Equals(d2)}");  // True（名稱和大小都一樣）
// Console.WriteLine($"d1 == d3：{d1.Equals(d3)}");    // False

// ──────────────────────────────
// 顧客介面：定義「面向顧客」的功能契約
// 任何實作此介面的類別，都必須提供這兩個方法
// 命名慣例：介面名稱前面加「I」，是 C# 業界規定的命名慣例（非語法要求）
// 目的是讓人一眼就能區分「介面」和「類別」，例如：
//   ICustomerFacing → I 開頭 = 介面
//   DrinkShopStaff  → 沒有 I = 類別
// .NET 內建型別也遵守此慣例：IEnumerable、IDisposable、IEquatable<T> ...
// public interface ICustomerFacing
// {
//     void ShowMenu();                    // 展示菜單
//     void TakePayment(decimal amount);   // 收款
// }

// 廚房介面：定義「面向廚房」的功能契約
// 這些操作屬於內部流程，不應該讓顧客直接呼叫
// public interface IKitchenFacing
// {
//     void TakeOrder(string item, int quantity);  // 傳遞訂單給廚房
//     void NotifyKitchen();                       // 通知廚房出餐
// }

// ── 繼承 vs 實作 用詞比較 ──
// | 情況              | 說法            | 符號 |
// | class 對 interface | 實作 implement  |  :   |
// | class 對 class     | 繼承 inherit    |  :   |
// 雖然都是 : 但意思不同，面試時說錯用詞會扣分！

// 店員類別：同時實作 ICustomerFacing 和 IKitchenFacing 兩個介面
// 語法：class 類別名稱 : 介面1, 介面2
// public class DrinkShopStaff : ICustomerFacing, IKitchenFacing
// {
//     private string _name;  // 私有欄位：儲存店員名字

//     // 建構子：建立物件時傳入店員名字
//     public DrinkShopStaff(string name) => _name = name;

//     // ── ICustomerFacing 實作（一般實作，加 public，可直接呼叫）──

//     // 展示菜單給顧客看
//     public void ShowMenu() =>
//         Console.WriteLine($"[{_name}] 菜單：珍珠奶茶 $60 / 綠茶 $40 / 拿鐵 $80");

//     // 向顧客收款
//     public void TakePayment(decimal amount) =>
//         Console.WriteLine($"[{_name}] 收款 ${amount}，謝謝光臨！");

//     // ── IKitchenFacing 明確介面實作（不加 public，外部無法直接呼叫）──
//     // 語法：void 介面名稱.方法名稱() { ... }
//     // 效果：這個方法只有在「以 IKitchenFacing 型別看待」時才看得到

//     // 傳遞訂單給廚房
//     void IKitchenFacing.TakeOrder(string item, int quantity) =>
//         Console.WriteLine($"[廚房訂單] {item} x{quantity}，請備料");

//     // 通知廚房有新訂單
//     void IKitchenFacing.NotifyKitchen() =>
//         Console.WriteLine("[廚房通知] 新訂單已送出，請出餐");
// }

// 飲料類別：實作 IEquatable<Drink>，自訂兩杯飲料的比較邏輯
// 語法：class 類別名稱 : IEquatable<自己>
// public class Drink : IEquatable<Drink>
// {
//     public string Name { get; set; } = "";  // 飲料名稱
//     public string Size { get; set; } = "";  // 杯型

//     // 實作 Equals：只要名稱和杯型都相同，就視為同一杯飲料
//     // Tuple 比較語法：(a, b) == (c, d)  →  a==c 且 b==d
//     public bool Equals(Drink? other) =>
//         (this.Name, this.Size) == (other?.Name, other?.Size);
// }

