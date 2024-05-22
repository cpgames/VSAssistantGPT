namespace SampleSolution
{
    internal class Fib : IFib
    {
        /// <summary>
        /// Constructor for the Fib class
        /// </summary>
        public Fib()
        {
            // Default constructor
        }

        public int FibonacciRecursive(int n)
        {
            /// <summary>
            /// Method to calculate the Fibonacci series recursively
            /// </summary>
            /// <param name="n">the index of the Fibonacci number to calculate</param>
            /// <returns>the Fibonacci number at index n</returns>
            if (n <= 1)
                return n;
            else
                return FibonacciRecursive(n - 1) + FibonacciRecursive(n - 2);
        }

        public int FibonacciNonRecursive(int n)
        {
            // Initialize variables for Fibonacci sequence
            int a = 0;
            int b = 1;

            // Base case check: if input index is 0, return the first Fibonacci number
            if (n == 0)
                return a;

            // Calculate Fibonacci number iteratively up to index n
            for (int i = 2; i <= n; i++)
            {
                int temp = a + b;
                a = b;
                b = temp;
            }

            // Return the Fibonacci number at index n
            return b;
        }
    }
}
