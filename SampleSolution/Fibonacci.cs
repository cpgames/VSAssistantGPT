public static class Fibonacci
{
    /// <summary>
    /// Computes the nth Fibonacci number in a non-recursive manner.
    /// </summary>
    /// <param name="n">The position in the Fibonacci sequence.</param>
    /// <returns>The nth Fibonacci number.</returns>
    public static int Compute(int n)
    {
        // Base case: the first and second Fibonacci numbers are 0 and 1, respectively
        if (n <= 1) return n;

        int a = 0; // Variable to store (n-2)th Fibonacci number
        int b = 1; // Variable to store (n-1)th Fibonacci number
        int c = 0; // Variable to store nth Fibonacci number
                   // Loop to calculate the nth Fibonacci number
        for (int i = 2; i <= n; i++)
        {
            c = a + b; // Calculate the next Fibonacci number
            a = b;     // Update (n-2)th number to (n-1)th number
            b = c;     // Update (n-1)th number to nth number
        }
        return c; // Return the nth Fibonacci number
    }
}