namespace Walk.Ide.Wasm.Facts;

public partial class InitialSolutionFacts
{
    public const string PERSON_CS_ABSOLUTE_FILE_PATH = @"/BlazorCrudApp/BlazorCrudApp.Wasm/Persons/Person.cs";
    public const string PERSON_CS_CONTENTS =
"""""""""
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/arrays

namespace Walk.CompilerServices.CSharp;

// Wrapping with a code block will show me if the array syntax breaks code block parsing.
public void LR_Arrays_A()
{
    // # Arrays
    // 12/14/2024
    
    // In this article
    //    Single-dimensional arrays
    //    Multidimensional arrays
    //    Jagged arrays
    //    Implicitly typed arrays
    
    type[] arrayName;
    
    type?[] arrayName;
    type[]? arrayName;
    type?[]? arrayName;
    
    int[] numbers = new int[10];
    string[] messages = new string[10];
    
    int[] array1 = new int[5];
    
    int[] array2 = [1, 2, 3, 4, 5, 6];
    
    int[,] multiDimensionalArray1 = new int[2, 3];
    
    int[,] multiDimensionalArray2 = { { 1, 2, 3 }, { 4, 5, 6 } };
    
    int[][] jaggedArray = new int[6][];
    
    jaggedArray[0] = [1, 2, 3, 4];
    
    int[] array = [1, 2, 3, 4, 5, 6];
    int[] array2 = {1, 2, 3, 4, 5, 6};
    
    int[] array = new int[5];
    string[] weekDays = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    Console.WriteLine(weekDays[0]);
    Console.WriteLine(weekDays[1]);
}

class ArrayExample
{
    static void DisplayArray(string[] arr) => Console.WriteLine(string.Join(" ", arr));

    static void ChangeArray(string[] arr) => Array.Reverse(arr);

    static void ChangeArrayElements(string[] arr)
    {
        arr[0] = "Mon";
        arr[1] = "Wed";
        arr[2] = "Fri";
    }

    static void Main()
    {
        string[] weekDays = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
        DisplayArray(weekDays);
        Console.WriteLine();

        ChangeArray(weekDays);
        Console.WriteLine("Array weekDays after the call to ChangeArray:");
        DisplayArray(weekDays);
        Console.WriteLine();

        ChangeArrayElements(weekDays);
        Console.WriteLine("Array weekDays after the call to ChangeArrayElements:");
        DisplayArray(weekDays);
    }
}

// Wrapping with a code block will show me if the array syntax breaks code block parsing.
public void LR_Arrays_B()
{
    int[,] array2DDeclaration = new int[4, 2];
    
    int[,,] array3DDeclaration = new int[4, 2, 3];
    
    int[,] array2DInitialization =  { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } };
    int[,,] array3D = new int[,,] { { { 1, 2, 3 }, { 4,   5,  6 } },
                                    { { 7, 8, 9 }, { 10, 11, 12 } } };
    
    System.Console.WriteLine(array2DInitialization[0, 0]);
    System.Console.WriteLine(array2DInitialization[0, 1]);
    System.Console.WriteLine(array2DInitialization[1, 0]);
    System.Console.WriteLine(array2DInitialization[1, 1]);
    
    System.Console.WriteLine(array2DInitialization[3, 0]);
    System.Console.WriteLine(array2DInitialization[3, 1]);
    
    System.Console.WriteLine(array3D[1, 0, 1]);
    System.Console.WriteLine(array3D[1, 1, 2]);
    
    var allLength = array3D.Length;
    var total = 1;
    for (int i = 0; i < array3D.Rank; i++)
    {
        total *= array3D.GetLength(i);
    }
    System.Console.WriteLine($"{allLength} equals {total}");
    
    int[,] numbers2D = { { 9, 99 }, { 3, 33 }, { 5, 55 } };
    
    foreach (int i in numbers2D)
    {
        System.Console.Write($"{i} ");
    }
    
    int[,,] array3D = new int[,,] { { { 1, 2, 3 }, { 4,   5,  6 } },
                            { { 7, 8, 9 }, { 10, 11, 12 } } };
    foreach (int i in array3D)
    {
        System.Console.Write($"{i} ");
    }
    
    int[,,] array3D = new int[,,] { { { 1, 2, 3 }, { 4,   5,  6 } },
                            { { 7, 8, 9 }, { 10, 11, 12 } } };
    
    for (int i = 0; i < array3D.GetLength(0); i++)
    {
        for (int j = 0; j < array3D.GetLength(1); j++)
        {
            for (int k = 0; k < array3D.GetLength(2); k++)
            {
                System.Console.Write($"{array3D[i, j, k]} ");
            }
            System.Console.WriteLine();
        }
        System.Console.WriteLine();
    }
}
    
// Wrapping with a code block will show me if the array syntax breaks code block parsing.
public void LR_Arrays_C()
{
    static void Print2DArray(int[,] arr)
    {
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                System.Console.WriteLine($"Element({i},{j})={arr[i,j]}");
            }
        }
    }
    static void ExampleUsage()
    {
        Print2DArray(new int[,] { { 1, 2 }, { 3, 4 }, { 5, 6 }, { 7, 8 } });
    }
    
    int[][] jaggedArray = new int[3][];
    
    jaggedArray[0] = [1, 3, 5, 7, 9];
    jaggedArray[1] = [0, 2, 4, 6];
    jaggedArray[2] = [11, 22];
    
    int[][] jaggedArray2 =
    [
        [1, 3, 5, 7, 9],
        [0, 2, 4, 6],
        [11, 22]
    ];
    
    jaggedArray2[0][1] = 77;
    
    jaggedArray2[2][1] = 88;
    
    int[][,] jaggedArray3 =
    [
        new int[,] { {1,3}, {5,7} },
        new int[,] { {0,2}, {4,6}, {8,10} },
        new int[,] { {11,22}, {99,88}, {0,9} }
    ];
    
    Console.Write("{0}", jaggedArray3[0][1, 0]);
    Console.WriteLine(jaggedArray3.Length);
    
    int[][] arr = new int[2][];
    
    arr[0] = [1, 3, 5, 7, 9];
    arr[1] = [2, 4, 6, 8];
    
    for (int i = 0; i < arr.Length; i++)
    {
        System.Console.Write("Element({0}): ", i);
    
        for (int j = 0; j < arr[i].Length; j++)
        {
            System.Console.Write("{0}{1}", arr[i][j], j == (arr[i].Length - 1) ? "" : " ");
        }
        System.Console.WriteLine();
    }
    
    int[] a = new[] { 1, 10, 100, 1000 };
    
    Console.WriteLine("First element: " + a[0]);
    Console.WriteLine("Second element: " + a[1]);
    
    var b = new[] { "hello", null, "world" };
    
    Console.WriteLine(string.Join(" ", b));
    
    int[][] c =
    [
        [1,2,3,4],
        [5,6,7,8]
    ];
    for (int k = 0; k < c.Length; k++)
    {
        for (int j = 0; j < c[k].Length; j++)
        {
            Console.WriteLine($"Element at c[{k}][{j}] is: {c[k][j]}");
        }
    }
    
    string[][] d =
    [
        ["Luca", "Mads", "Luke", "Dinesh"],
        ["Karen", "Suma", "Frances"]
    ];
    
    int i = 0;
    foreach (var subArray in d)
    {
        int j = 0;
        foreach (var element in subArray)
        {
            Console.WriteLine($"Element at d[{i}][{j}] is: {element}");
            j++;
        }
        i++;
    }
    
    var contacts = new[]
    {
        new
        {
            Name = "Eugene Zabokritski",
            PhoneNumbers = new[] { "206-555-0108", "425-555-0001" }
        },
        new
        {
            Name = "Hanying Feng",
            PhoneNumbers = new[] { "650-555-0199" }
        }
    };
}

""""""""";
}
