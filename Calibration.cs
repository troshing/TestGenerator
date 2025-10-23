using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAKompensator
{
    public class Calibration
    {
        private float _u1Plus;
        private float _u1Minus;
        private float _u2Plus;
        private float _u2Minus;

        private short _u1PlusCode;
        private short _u1MinusCode;
        private short _u2PlusCode;
        private short _u2MinusCode;

        public float BUp;
        public float BUm;
        public float KUp;
        public float KUm;


        public Calibration()
        {
            InitValues();
        }
        public void Set_UPlus(float u1, float u2)
        {
            _u1Plus = u1;
            _u2Plus = u2;
        }
        public void Set_U1Plus(float u1)
        {
            _u1Plus = u1;
        }
        
        public void Set_U2Plus(float u2)
        {
            _u2Plus = u2;
        }

        public void Set_UMinus(float u1, float u2)
        {
            _u1Minus = u1;
            _u2Minus = u2;
        }
        public void Set_U1Minus(float u1)
        {
            _u1Minus = u1;
        }
        public void Set_U2Minus(float u2)
        {
            _u2Minus = u2;
        }
        public void Set_UPlusCode(short u1, short u2)
        {
            _u1PlusCode = u1;
            _u2PlusCode = u2;
        }
        public void Set_U1PlusCode(short u1)
        {
            _u1PlusCode = u1;
        }
        public void Set_U2PlusCode(short u2)
        {
            _u2PlusCode = u2;
        }
        public void Set_UMinusCode(short u1, short u2)
        {
            _u1MinusCode = u1;
            _u2MinusCode = u2;
        }
        public void Set_U1MinusCode(short u1)
        {
            _u1MinusCode = u1;
        }
        public void Set_U2MinusCode( short u2)
        {
            _u2MinusCode = u2;
        }
        public void Calc_Koeff_Uplus()
        {
            // BUp = (_u2Plus * _u1PlusCode - _u1Plus * _u2PlusCode) / (_u1PlusCode - _u2PlusCode);
            KUp = _u2Plus / _u2PlusCode; // (_u1Plus - BUp) / _u1PlusCode;
        }

        public void Calc_Koeff_Uminus()
        {
            // BUm = (_u2Minus * _u1MinusCode - _u1Minus * _u2MinusCode) / (_u1MinusCode - _u2MinusCode);
            KUm = _u2Minus / _u2MinusCode;  // (_u1Minus - BUm) / _u1MinusCode;
        }

        public void InitValues()
        {
            _u1Plus = 0;
            _u1Minus = 0;
            _u2Plus = 0;
            _u2Minus = 0;

            _u1PlusCode = 0;
            _u1MinusCode = 0;
            _u2PlusCode = 0;
            _u2MinusCode = 0;

            BUp = 0.0f;
            BUm = 0.0f;
            KUp = 0.0f;
            KUm = 0.0f;
        }
    }
}
