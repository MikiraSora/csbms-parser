using System;
using System.Collections.Generic;
using System.Text;

namespace CSBMSParser
{
    /**
     * 
     * 
     * @author exch
     */
    public class DecodeLog
    {
        private string message;

        private State state;

        public DecodeLog(State state, string message)
        {
            this.message = message;
            this.state = state;
        }

        public State getState()
        {
            return state;
        }

        public string getMessage()
        {
            return message;
        }

        public enum State
        {
            INFO, WARNING, ERROR
        }
    }
}
