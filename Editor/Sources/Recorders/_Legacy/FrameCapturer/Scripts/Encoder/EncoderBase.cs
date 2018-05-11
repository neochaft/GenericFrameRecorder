using System;


namespace FrameCapturer
{
    public abstract class EncoderBase
    {
        public EncoderBase()
        {
            AppDomain.CurrentDomain.DomainUnload += WaitAsyncDelete;
        }
        public static void WaitAsyncDelete(object sender, EventArgs e)
        {
            fcAPI.fcWaitAsyncDelete();
        }

        public abstract void Release();
        public abstract bool IsValid();
    }

}
