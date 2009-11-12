extern alias RapidEngine;
using System;
using System.Collections.Generic;
using System.Text;


namespace DeconTools.Backend.Core
{
    public class Rapid                    //this is no longer used....
    {
        private static Rapid instance;


        public Rapid()
        {
            this.RapidTransformer = new RapidEngine.Decon2LS.HornTransform.clsHornTransform();

        }


        public static Rapid getInstance()
        {
            if (instance == null)
            {
                instance = new Rapid();
            }
            return instance;

        }


        public RapidEngine.Decon2LS.HornTransform.clsHornTransform RapidTransformer;


        



        public void Initialize()
        {
        

        }

    }
}
