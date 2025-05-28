using System;
using System.Threading.Tasks;
using System.Windows;

namespace UiStore.Common
{
    using System;
    using System.Windows;
    using System.Windows.Threading;

    public static class DispatcherHelper
    {
        /// <summary>
        /// Kiểm tra xem có đang ở UI thread hay không.
        /// </summary>
        public static bool CheckAccess()
        {
            return Application.Current?.Dispatcher?.CheckAccess() ?? false;
        }

        /// <summary>
        /// Thực thi 1 hành động trên UI thread đồng bộ (blocking).
        /// </summary>
        public static void RunOnUI(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.Invoke(action);
            }
        }

        /// <summary>
        /// Thực thi 1 hàm trả về giá trị trên UI thread đồng bộ (blocking).
        /// </summary>
        public static T RunOnUI<T>(Func<T> func)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                return func();
            }
            else
            {
                return dispatcher.Invoke(func);
            }
        }

        /// <summary>
        /// Thực thi 1 hành động trên UI thread bất đồng bộ (fire and forget).
        /// </summary>
        public static void RunAsyncOnUI(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher == null || dispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                dispatcher.BeginInvoke(action);
            }
        }
    }


}
