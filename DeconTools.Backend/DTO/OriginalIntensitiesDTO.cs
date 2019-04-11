using DeconTools.Backend.Core;

namespace DeconTools.Backend.DTO
{
    public class OriginalIntensitiesDTO
    {

        private IsosResult isosResult;


        public OriginalIntensitiesDTO(IsosResult isosResult)
        {
            this.isosResult = isosResult;
        }

        public double originalIntensity { get; set; }

        public double totIsotopicOrginalIntens { get; set; }


    }
}
