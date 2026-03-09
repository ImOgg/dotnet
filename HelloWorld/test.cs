// // ⚠️ 注意：若要執行此檔案（test.cs），
// // 必須將 Program.cs 中所有的頂層語句（top-level statements）全部註解掉。
// // 原因：一個 .NET 專案只能有一個檔案包含可執行的頂層語句，
// // 否則編譯器不知道要從哪裡開始執行，會產生編譯錯誤。

// // Console.WriteLine("Hello, World!");

// using System;
// using System.Collections.Generic;

// class Book
// {
//     public string Title { get; }
//     public string Author { get; }
//     public bool IsBorrowed { get; private set; }

//     public Book(string title, string author)
//     {
//         Title = title;
//         Author = author;
//         IsBorrowed = false;
//     }

//     public void Borrow()
//     {
//         if (!IsBorrowed)
//         {
//             IsBorrowed = true;
//             Console.WriteLine($"《{Title}》 已成功借出！");
//         }
//         else
//         {
//             Console.WriteLine($"《{Title}》 已被借走，無法借出。");
//         }
//     }

//     public void Return()
//     {
//         if (IsBorrowed)
//         {
//             IsBorrowed = false;
//             Console.WriteLine($"《{Title}》 已歸還！");
//         }
//         else
//         {
//             Console.WriteLine($"《{Title}》 原本就未被借出。");
//         }
//     }
// }

// class Library
// {
//     private List<Book> books = new List<Book>();

//     public void AddBook(Book book)
//     {
//         books.Add(book);
//         Console.WriteLine($"成功新增書籍：《{book.Title}》 作者：{book.Author}");
//     }

//     public void ShowBooks()
//     {
//         Console.WriteLine("\n📚 圖書館藏書：");
//         foreach (var book in books)
//         {
//             string status = book.IsBorrowed ? "已借出" : "可借閱";
//             Console.WriteLine($"- {book.Title} by {book.Author} ({status})");
//         }
//     }
// }

// class Program
// {
//     static void Main()
//     {
//         Library library = new Library();

//         // 新增書籍
//         library.AddBook(new Book("C# 入門", "張小明"));
//         library.AddBook(new Book("物件導向設計", "李大華"));

//         // 顯示館藏
//         library.ShowBooks();

//         // 借書
//         library.ShowBooks();
//         Console.WriteLine();
//         var borrowedBook = new Book("C# 入門", "張小明"); // ⚠️ 這裡只是示範，實際應該從館藏取書
//         borrowedBook.Borrow();

//         // 還書
//         borrowedBook.Return();
//     }
// }

// // try catch 是 C# 中用來處理例外（exceptions）的語法結構。
// // 當程式執行過程中發生錯誤（例如除以零、檔案不存在等），會拋出一個例外物件，
// // 這時候就可以使用 try-catch 來捕捉並處理這些例外，避免程式崩潰。

// public class ExceptionTest
// {
//     static double SafeDivision(double x, double y)
//     {
//         if (y == 0)
//             throw new DivideByZeroException();
//         return x / y;
//     }

//     public static void Main()
//     {
//         // Input for test purposes. Change the values to see
//         // exception handling behavior.
//         double a = 98, b = 0;
//         double result;

//         try
//         {
//             result = SafeDivision(a, b);
//             Console.WriteLine($"{a} divided by {b} = {result}");
//         }
//         catch (DivideByZeroException)
//         {
//             Console.WriteLine("Attempted divide by zero.");
//         }
//     }
// }

// LINQ 

// using System;
// using System.Linq; // 一定要引用這個命名空間

// 使用 LINQ 查詢陣列中的偶數
// class Program
// {
//     static void Main()
//     {
//         int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

//         // 使用 LINQ 查詢偶數
//         var evenNumbers = from n in numbers
//                           where n % 2 == 0
//                           select n;

//         Console.WriteLine("偶數有：");
//         foreach (var n in evenNumbers)
//         {
//             Console.WriteLine(n);
//         }
//     }
// }