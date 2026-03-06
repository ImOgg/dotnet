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

public static class SquareExample
{
    // 程式進入點
    public static void Main()
    {
        // 用變數呼叫 Square：計算 4 的平方
        int num = 4;
        int productA = Square(num);       // productA = 16
        Console.WriteLine($"The square of {num} is {productA}.");
        // 用整數字面值呼叫 Square：計算 12 的平方
        int productB = Square(12);        // productB = 144
        Console.WriteLine($"The square of 12 is {productB}.");
        // 用運算式呼叫 Square：計算 (16 * 3) = 48 的平方
        int productC = Square(productA * 3);  // productC = 2304
        Console.WriteLine($"The square of {productA} * 3 is {productC}.");
        // ⚠️ 注意：這裡只有計算，沒有 Console.WriteLine()
        //    所以執行後 terminal 不會有任何輸出！
    }

    // 計算整數的平方（i * i）並回傳結果
    static int Square(int i)
    {
        int input = i;
        return input * input;
    }
}

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