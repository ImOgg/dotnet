// See https://aka.ms/new-console-template for more information

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
public class Program
{
    // 方法（method）不能獨立存在，必須放在類別（class）裡面。
    public static void Main()
    {
        Point pt = new Point(1, 1);
        var pt2 = pt with { Y = 10 };
        Console.WriteLine($"The two points are {pt} and {pt2}");
    }
}

// record 是 C# 9 引入的型別，專門用來表示「不可變的資料」
// 語法：public record 名稱(屬性1, 屬性2, ...)
// 特性：
//   1. 自動產生建構子、getter、ToString()、Equals() 等方法
//   2. 不可變（immutable）：建立後屬性值無法直接修改
//   3. with 語法：可以複製一個 record，並只改變指定的屬性
//      例：var pt2 = pt with { Y = 10 };  → 複製 pt，但把 Y 改成 10
//   4. 與 class 的差異：record 比較值（value equality），class 比較參考（reference equality）
public record Point(int X, int Y);
