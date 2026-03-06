// See https://aka.ms/new-console-template for more information
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

// ============================
// 9. 類別、物件與屬性（Class, Object & Property）
// ============================
// Top-level statements 必須放在型別宣告之前

// 用有參數建構子建立物件（FirstName 由建構子設定，其餘用初始化語法補上）
var p1 = new Person("Grace") { LastName = "Hopper", Age = 85 };
// 用無參數建構子建立物件（required 屬性全部用初始化語法設定）
var p2 = new Person() { FirstName = "Ada", LastName = "Lovelace", Age = 36 };

p1.Nickname = "  Admiral Grace  ";  // setter 會自動 Trim() 去除空白
p1.SetEmail("grace@navy.mil");      // Email 是 private set，只能透過方法修改
p1.Login();
p1.Login();

Console.WriteLine("--- 基本屬性 ---");
Console.WriteLine(p1);                                  // 輸出：[Person] Grace Hopper, Age: 85
Console.WriteLine(p2);                                  // 輸出：[Person] Ada Lovelace, Age: 36

Console.WriteLine("\n--- 自訂 setter / Trim ---");
Console.WriteLine($"Nickname: '{p1.Nickname}'");        // 輸出：'Admiral Grace'（已去除空白）

Console.WriteLine("\n--- private set ---");
Console.WriteLine($"Email: {p1.Email}");                // 輸出：grace@navy.mil
// p1.Email = "xxx";                                    // ❌ 編譯錯誤：Email 是 private set

Console.WriteLine("\n--- 唯讀屬性（Read-only）---");
Console.WriteLine($"CreatedAt: {p1.CreatedAt}");        // 只有 p1 有值（用有參數建構子建立）
Console.WriteLine($"CreatedAt: {p2.CreatedAt}");        // 輸出：01/01/0001（DateTime 預設值，無參數建構子未設定）

Console.WriteLine("\n--- Lazy Loading FullName 快取 ---");
Console.WriteLine($"FullName: {p1.FullName}");          // 輸出：Grace Hopper（第一次計算並快取）
p1.LastName = "Murray";                                 // 修改 LastName → 清空快取
Console.WriteLine($"FullName: {p1.FullName}");          // 輸出：Grace Murray（重新計算）

Console.WriteLine("\n--- 方法 ---");
Console.WriteLine($"LoginCount: {p1.GetLoginCount()}"); // 輸出：2

Console.Write($"\nPress Enter to exit...");
Console.Read();

// 型別宣告放在 top-level statements 之後
public class Person
{
    // 私有欄位（Field）：不對外公開，搭配屬性或方法使用
    private int _loginCount = 0;
    private string? _nickname;
    private string? _lastName;
    private string? _fullName;  // FullName 的快取欄位

    // 無參數建構子（Parameterless Constructor）
    public Person() { }

    // 有參數建構子 + [SetsRequiredMembers]：
    // 告訴編譯器此建構子已滿足所有 required 屬性，呼叫端不需再補給
    [SetsRequiredMembers]
    public Person(string firstName)
    {
        FirstName = firstName;
        CreatedAt = DateTime.Now;  // 唯讀屬性只能在建構子裡賦值
    }

    // required + init：建立時必須給值，建立後不可修改（唯讀）
    public required string FirstName { get; init; }

    // required + 自訂 setter：修改 LastName 時清空 FullName 快取
    public required string LastName
    {
        get => _lastName;
        set
        {
            _lastName = value;
            _fullName = null;  // 清空快取，讓 FullName 下次重新計算
        }
    }

    // 預設值屬性 + private set：外部只能讀，只能透過類別內部方法修改
    public string Email { get; private set; } = string.Empty;
    public void SetEmail(string email) => Email = email;  // 透過方法修改 Email

    // 自訂 setter：寫入時自動去除前後空白
    public string? Nickname
    {
        get => _nickname;
        set => _nickname = value?.Trim();
    }

    // 唯讀屬性（Read-only）：只有 get，只能在建構子設值，外部完全不能修改
    public DateTime CreatedAt { get; }

    // Lazy Loading 計算屬性：第一次呼叫才計算並快取，LastName 改變時會重新計算
    public string FullName
    {
        get
        {
            // ??= 複合賦值：若 _fullName 是 null 才計算並賦值，等同 if (_fullName is null) _fullName = ...
            _fullName ??= $"{FirstName} {LastName}";
            return _fullName;
        }
    }

    // 一般自動屬性
    public int Age { get; set; }

    // 方法：操作私有欄位
    public void Login() => _loginCount++;
    public int GetLoginCount() => _loginCount;

    // 覆寫 ToString()：讓 Console.WriteLine(物件) 有意義的輸出
    public override string ToString() =>
        $"[Person] {FullName}, Age: {Age}";
}

