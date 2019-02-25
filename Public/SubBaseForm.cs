using AFT_System.CustomControl.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT_System.Public
{
    public class SubBaseForm
    {
        #region 页面加载特效

        private FrmLoading formloading = null;
        private object parmLoading = null;
        private object resultLoading = null;

        public delegate object DelegateHasParmAndHasReturn(object parm);
        public delegate object DelegateNonParmAndHasReturn();
        public delegate void DelegateHasParmAndNonReturn(object parm);
        public delegate void DelegateNonParmAndNonReturn();

        private DelegateHasParmAndHasReturn hasParmAndHasReturn = null;
        private DelegateNonParmAndHasReturn nonParmAndHasReturn = null;
        private DelegateHasParmAndNonReturn hasParmAndNonReturn = null;
        private DelegateNonParmAndNonReturn nonParmAndNonReturn = null;

        private string Desc = string.Empty;

        public SubBaseForm(string desc)
        {
            Desc = desc;
        }
        /// <summary>
        /// 有参数并且有返回值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="para"></param>
        /// <returns></returns>
        public object HasParmAndHasReturnMethod(DelegateHasParmAndHasReturn method, object para = null)
        {
            parmLoading = para;
            hasParmAndHasReturn = method;
            formloading = new FrmLoading();
            formloading.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorkerLoad_HasParmAndHasReturn);
            SetDisplayText(Desc);
            formloading.ShowDialog();
            formloading.Close();
            return resultLoading;
        }

        /// <summary>
        /// 无参数并且有返回值
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public object NonParmAndHasReturnMethod(DelegateNonParmAndHasReturn method)
        {
            nonParmAndHasReturn = method;
            formloading = new FrmLoading();
            formloading.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorkerLoad_NonParmAndHasReturn);
            SetDisplayText(Desc);
            formloading.ShowDialog();
            formloading.Close();
            return resultLoading;
        }
        /// <summary>
        /// 有参数并且无返回值
        /// </summary>
        /// <param name="method"></param>
        /// <param name="para"></param>
        public void HasParmAndNonReturnMethod(DelegateHasParmAndNonReturn method, object para = null)
        {
            parmLoading = para;
            hasParmAndNonReturn = method;
            formloading = new FrmLoading();
            formloading.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorkerLoad_HasParmAndNonReturn);
            SetDisplayText(Desc);
            formloading.ShowDialog();
            formloading.Close();
        }


        /// <summary>
        /// 无参数并且无返回值
        /// </summary>
        /// <param name="method"></param>
        public void NonParmAndNonReturnMethod(DelegateNonParmAndNonReturn method)
        {
            nonParmAndNonReturn = method;
            formloading = new FrmLoading();
            formloading.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorkerLoad_NonParmAndNonReturn);
            SetDisplayText(Desc);
            formloading.ShowDialog();
            formloading.Close();
        }
        /// <summary>
        /// 有参数并且有返回值
        /// </summary>
        /// <param name="method"></param>
        private void backgroundWorkerLoad_HasParmAndHasReturn(object sender, DoWorkEventArgs e)
        {
            resultLoading = hasParmAndHasReturn(parmLoading);
        }

        /// <summary>
        /// 无参数有返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLoad_NonParmAndHasReturn(object sender, DoWorkEventArgs e)
        {
            resultLoading = nonParmAndHasReturn();
        }

        /// <summary>
        /// 有参数无返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLoad_HasParmAndNonReturn(object sender, DoWorkEventArgs e)
        {
            hasParmAndNonReturn(parmLoading);
        }

        /// <summary>
        /// 无参数无返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorkerLoad_NonParmAndNonReturn(object sender, DoWorkEventArgs e)
        {
            nonParmAndNonReturn();
        }


        //#region 下载附件进度条
        //private ShowLoadDialogForm formdownload = null;
        //protected delegate void DelegateNonCanAndNonReturn();
        //private DelegateNonCanAndNonReturn nonCanAndNonReturn = null;
        //protected void NonCanAndNonReturnMethod(DelegateNonCanAndNonReturn method)
        //{
        //    nonCanAndNonReturn = method;
        //    formdownload = new ShowLoadDialogForm();
        //    formdownload.backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_NonCanAndNonReturn);

        //    formdownload.ShowDialog();
        //    formdownload.Dispose();
        //}
        //private void backgroundWorker1_NonCanAndNonReturn(object sender, DoWorkEventArgs e)
        //{
        //    nonCanAndNonReturn();
        //}
        //#endregion
        /// <summary>
        /// 设置进度条显示的文字
        /// </summary>
        /// <param name="desc"></param>
        private void SetDisplayText(string desc)
        {
            if (!String.IsNullOrEmpty(desc) && formloading != null)
            {
                formloading.SetProgressText(desc);
            }
        }
        #endregion
    }
}
