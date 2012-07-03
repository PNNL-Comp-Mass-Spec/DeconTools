
namespace DeconTools.Backend.Core
{
    public abstract class Task
    {

        public abstract void Execute(ResultCollection resultList);

        public virtual string Name {get;set;}
        
        public virtual void Cleanup()
        {
            return;
        }


    }
}
