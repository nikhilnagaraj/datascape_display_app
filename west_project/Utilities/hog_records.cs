using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace west_project.Utilities
{

    //This class is used to maintain the Histogram of Oriented Gradient Records to be displayed on the charts. It contains two properties, the angle
    //and the number of gradients at that angle.
    public class hog_records
    {
        public double angle;
        public double gradient_num;

        public static hog_records createHOGData(double angle, double gradient_num)
        {
            hog_records hogdata = new hog_records();
            hogdata.angle = angle;
            hogdata.gradient_num = gradient_num;
            return hogdata;
        }

    }
}
