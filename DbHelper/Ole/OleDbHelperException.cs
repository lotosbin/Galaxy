using System;
using System.Runtime.Serialization;

namespace Glavesoft.SmartData.DbHelper.Ole
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class OleDbHelperException : ApplicationException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbHelperException"/> class.
        /// </summary>
        public OleDbHelperException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbHelperException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public OleDbHelperException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbHelperException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public OleDbHelperException(string message,
                                    Exception inner)
            : base(message,
                   inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OleDbHelperException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected OleDbHelperException(
            SerializationInfo info,
            StreamingContext context)
            : base(info,
                   context)
        {
        }
    }
}