using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using Polly;

namespace ColPolly
{
    internal class Program
    {
        //https://zhuanlan.zhihu.com/p/225377155
        private static void Main(string[] args)
        {
            // Polly 的七种策略：重试、断路、超时、隔离、回退、缓存、策略包。
            // 重试（Retry）：出现故障自动重试。
            // 断路（Circuit-breaker）：当系统遇到严重问题时，快速回馈失败比让用户/调用者等待要好，限制系统出错的体量，有助于系统恢复。 当第三方 API，长时间没有响应，当系统出错的次数超过了指定的阀值，就要中断当前线路，等待一段时间后再继续。
            // 超时（Timeout）：当系统超过一定时间的等待，我们就几乎可以判断不可能会有成功的结果。
            // 隔离（Bulkhead Isolation）：当系统的一处出现故障时，可能触发多个失败调用，很容易耗尽主机的资源。
            // 回退（Fallback）：有些错误无法避免，就要有备用的方案。当无法避免的错误发生时，我们要有一个合理的返回来代替失败。比如很常见的一个场景是，当用户没有上传头像时，我们就给他一个默认头像
            // 缓存（Cache）：一般我们会把频繁使用且不会怎么变化的资源缓存起来，已提高系统的响应速度。如果不对缓存资源的调用进行封装，那么我们调用的时候就要先判断缓存中有没有这个资源，有的话就从缓存返回，否则就从资源存储的地方获取后缓存起来，再返回，而且有时还要考虑缓存过期和如何更新缓存的问题。
            // 策略包（Policy Wrap）：一种操作会有多种不同的故障，而不同的故障处理需要不同的策略。这些不同的策略必须包在一起，作为一个策略包，才能应用在同一种操作上。
            // 先是把预先定义好的多种不同的策略包在一起，作为一个整体策略，然后应用在同一个操作上。
            //var policyWrap = Policy
            //    .Wrap(fallback, cache, retry, breaker, timeout, bulkhead);
            //policyWrap.Execute(...);

            Policy
                //超时
                // 这里设置了超时时间不能超过 30秒，否则就认为是错误的结果，并执行回调
                //.Timeout(4, onTimeout: (context, timespan, task) =>
                // {
                //     Console.WriteLine("超时重试");
                // })
                // 隔离
                // 这个策略最多允许12个线程并发执行，如果执行被拒绝，则执行回调
                //.Bulkhead(12, context =>
                //{
                //})
                // 1.指定要处理什么异常 HTTPRequestException
                .Handle<HttpRequestException>()
                //// 或者指定需要处理什么样的错误返回
                .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.BadGateway)
                // 重试
                // 指定重试次数和重试策略 3 次
                //.Retry(3, (exception, retryCount, context) =>
                //{
                //    Console.WriteLine($"开始第 {retryCount} 次重试");
                //})
                //短路
                // 当系统出现两次某个异常时，就停下来，等待1分钟后再继续。
                .CircuitBreaker(2, TimeSpan.FromMinutes(1))
                //回退
                //.Fallback()
                // 执行具体任务
                .Execute(ExecuteMockRequest);

            Console.WriteLine("Hello World!");
        }

        /// <summary>
        /// 网络请求
        /// </summary>
        /// <returns></returns>
        private static HttpResponseMessage ExecuteMockRequest()
        {
            // 模拟网络请求
            Console.WriteLine("正在执行网络请求。。。。");
            Thread.Sleep(10000);

            // 模拟网络错误
            return new HttpResponseMessage(HttpStatusCode.BadGateway);
        }
    }
}