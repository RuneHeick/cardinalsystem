using Schare;

namespace CompilerTest
{
    public class B: Ishare
    {
        
        
        public static double k = 7;

        static B()
        {
            k = 800;
        }



        public string Test
        {
            get
            {
                return "Test2"; 
            }
            set
            {
                throw new System.NotImplementedException();
            }
        }
    }
}
