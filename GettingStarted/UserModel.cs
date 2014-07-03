using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GettingStarted
{
    public struct Position3D {
        public float X;
        public float Y;
        public float Z;
    };

    public class UserModel
    {
        public Position3D RightHandJointPosition { get; set; }
        public Position3D LeftHandJointPosition { get; set; }

        public Position3D HeadJointPosition { get; set; }

    }
}
