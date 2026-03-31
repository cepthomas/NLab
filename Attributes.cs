using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Ephemera.NBagOfTricks;
using Ephemera.NBagOfUis;

#pragma warning disable IDE0290 // Use primary constructor

namespace NLab
{
    // 1. Component interface
    public interface IService
    {
        void Operation();
    }

    // 2. Concrete Component
    public class ConcreteService : IService
    {
        public void Operation()
        {
            Console.WriteLine("ConcreteService.Operation() called.");
        }
    }

    // 3. Decorator (abstract base class)
    public abstract class ServiceDecorator : IService
    {
        protected readonly IService _service;

        public ServiceDecorator(IService service)
        {
            _service = service;
        }

        public virtual void Operation()
        {
            _service.Operation();
        }
    }

    // 4. Concrete Decorator (adds logging)
    public class LoggingDecorator : ServiceDecorator
    {
        public LoggingDecorator(IService service) : base(service) { }

        public override void Operation()
        {
            Console.WriteLine("LoggingDecorator: Before operation...");
            base.Operation(); // Call the wrapped object's operation
            Console.WriteLine("LoggingDecorator: After operation...");
        }
    }

    public class Attrs
    {
        public void Usage()
        {
            IService service = new ConcreteService();
            IService decoratedService = new LoggingDecorator(service);

            decoratedService.Operation();
        }
    }


    //////////////////////////////////////////////////////////////////////////////////


    // https://gist.github.com/dimitris-papadimitriou-chr/f5ade388a4e6b64fb1a9dbf0ab084756
    //namespace FunctionalExperimentation.SimpleRefactorings.Medium.Decorator1

    public static partial class FuntionalExtensions
    {
        /// <summary>
        /// Adds a debug loggin on the Task Result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static Task<T> AddDebugLogging<T>(this Task<T> @this) =>
            @this.ContinueWith(continuationFunction: (task) =>
            {
                var result = task.Result;
                Debug.WriteLine(result);
                return result;
            });
        public static Task<T> AddDebugLogging<T>(this Task<T> @this, Action<T> action) =>
            @this.ContinueWith(continuationFunction: (task) =>
            {
                var result = task.Result;
                action(result);
                return result;
            });
         
    }
}
