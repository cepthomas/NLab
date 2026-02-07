using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Reflection;
using static NLab.Utils;


namespace NLab
{
    /// <summary>TODO Experimental class to log enter/exit scope.</summary>
    public class Scoper : IDisposable
    {
        readonly string _func;
        //static readonly List<string> _captures = [];
        readonly int _tid;
        private bool disposedValue;

        //public static List<string> Captures { get { return _captures; } }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Scoper()
        {
            //// Get the caller-caller function info.
            //var sf1 = new StackFrame(1);
            //_func = sf1.GetMethod().Name;
            //var line = sf1.GetFileLineNumber();

            //var sf2 = new StackFrame(2);
            //_func = sf2.GetMethod().Name;
            //var line2 = sf2.GetFileLineNumber();

            //_func = callerMember;
            //var line = callerLine;

            // Get thread id.
            _tid = Environment.CurrentManagedThreadId;

            Tell(DBG, $"Scoper constructor T:{_tid}", 2);
            //_captures.Add($"{_func}<{_tid}>: Scoper constructor");
        }


        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public Scoper(
        //    [CallerFilePath] string callerFile = "",
        //    [CallerLineNumber] int callerLine = -1,
        //    [CallerMemberName] string callerMember = "???"
        //    )
        //{
        //    //// Get the caller-caller function info.
        //    //var sf1 = new StackFrame(1);
        //    //_func = sf1.GetMethod().Name;
        //    //var line = sf1.GetFileLineNumber();

        //    //var sf2 = new StackFrame(2);
        //    //_func = sf2.GetMethod().Name;
        //    //var line2 = sf2.GetFileLineNumber();

        //    _func = callerMember;
        //    var line = callerLine;

        //    // Get thread id.
        //    _tid = Environment.CurrentManagedThreadId;

        //    Tell(DBG, $"Scoper constructor F:{_func}({line}) T:{_tid}", 2);
        //    //_captures.Add($"{_func}<{_tid}>: Scoper constructor");
        //}



        ~Scoper()
        {
            Tell(DBG, $"Scoper destructor {_func} T!{_tid}", 1);
            //Tell(DBG, $"Scoper destructor {_func}({line}) T!{_tid}");
            //_captures.Add($"{_func}<{_tid}>: Scoper destructor");
        }

        public void Dispose()
        {
            Tell(DBG, $"Scoper dispose {_func} T{_tid}", 1);
            //Tell(DBG, $"Scoper dispose {_func}({line}) T{_tid}");
            //_captures.Add($"{_func}<{_tid}>: Scoper dispose");
        }




        ///////////////////////////////////////////////////////////////////////////////////////
        public bool TLOG_EQUAL<T>(T value1, T value2) where T : IComparable//,
            //[CallerFilePath] string file = "UNKNOWN_FILE",
            //[CallerLineNumber] int line = -1) where T : IComparable
        {
            bool pass = true;
            if (value1.CompareTo(value2) != 0)
            {
                //var fn = Path.GetFileName(file);
                //Tell(ERR, $"{fn} {line} [{value1}] should be [{value2}]", 2);
                Tell(ERR, $"[{value1}] should be [{value2}]", 2);
                pass = false;
            }
            //else
            //{
            //    RecordResult(true, $"", file, line);
            //}
            return pass;
        }

        // _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1, p2);
        // #define TLOG_INFO_2(text, p1, p2) _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1, p2);
        // #define TLOG_DETAIL_2(text, p1, p2) _logger.Trace(CMN_CTraceLogger::mkDetail, __LINE__, text, p1, p2);
        public void TLOG_INFO(string text)//,
            //[CallerFilePath] string file = "UNKNOWN_FILE",
            //[CallerLineNumber] int line = -1)
        {
            //var fn = Path.GetFileName(file);
            //Tell(INF, $"{fn} {line} [{text}]", 2);
            Tell(INF, $"{text}", 2);
        }
    }

    public static class TRACER
    {
        public delegate void Writer(string s);

        static Writer _writer; // = writer;

        // Do once in module:
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TLOG_INIT(Writer writer) 
        {
            _writer = writer;
        }


        // At function entry:
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void TLOG_CONTEXT()
        {
            //// Get the function name
            //var sf1 = new StackFrame(1);
            //var mname = sf1.GetMethod().Name;
            //var scoper = new Scoper(mname);

            using var scoper = new Scoper();

            // Get thread id
            int tid = Thread.GetCurrentProcessorId();
        }


        public static bool TLOG_EQUAL<T>(T value1, T value2,
            [CallerFilePath] string file = "UNKNOWN_FILE",
            [CallerLineNumber] int line = -1) where T : IComparable
        {
            bool pass = true;
            if (value1.CompareTo(value2) != 0)
            {
                Tell(ERR, $"[{value1}] should be [{value2}]", 2);
                pass = false;
            }
            //else
            //{
            //    RecordResult(true, $"", file, line);
            //}
            return pass;
        }


        // _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1, p2);
        // #define TLOG_INFO_2(text, p1, p2) _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1, p2);
        // #define TLOG_DETAIL_2(text, p1, p2) _logger.Trace(CMN_CTraceLogger::mkDetail, __LINE__, text, p1, p2);
        public static void TLOG_INFO(string text) //, params object[] vars)
        {
            Tell(INF, $"TLOG_INFO: [{text}]", 2);
        }



        /////////////////////////// old ////////////////////////////////////////////


        //// At function entry:
        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static void TLOG_CONTEXT_1(Func<object, object, bool> test) // object rs)
        //{
        //    // func is function name - from???
        //    // rs is return value for testing

        //    //Action<bool> test

        //    var sf1 = new StackFrame(1);
        //    var mname = sf1.GetMethod().Name;

        //    //MethodBase method = frame.GetMethod();
        //    //MethodBody body = method.GetMethodBody();
        //    //Console.WriteLine("Method: {0}", method.Name);
        //}




        //[MethodImpl(MethodImplOptions.NoInlining)]
        //public static void TLOG_CONTEXT(string func, object rs)
        //{
        //    // func is function name
        //    // rs is return value for testing
        //}


        // CMN_CTraceLogger _logger(#func, &rs);
        // /// Macro for declaring a context logger. Tests exit condition of RET_STAT and record failure.
        // #define TLOG_CONTEXT(func, rs) CMN_CTraceLogger _logger(#func, &rs);
        // /// Macro for declaring a context logger with timing information.
        // #define TLOG_CONTEXT_T(func, rs) CMN_CTraceLogger _logger(#func, &rs, CMN_TRUE);
        // /// Macro for declaring a simple context logger with no RET_STAT checking.
        // #define TLOG_CONTEXT_S(func) CMN_CTraceLogger _logger(#func);



        // Usage:
        // RET_STAT    RetStat = RET_STAT_NO_ERR;
        // TLOG_CONTEXT(CAssayData::UpdateSampleProgram, RetStat);
        // TLOG_INFO_1("No reprocessing should occur for Result %ld", this->xDbResRec.lResultID)
        // TLOG_INFO_3("CResultsData::UpdateSampleProgram: PROGRAM Adding reflex test %ld from Result %ld, Deferred:%hd",
        //                 pcNextTest->lAssayNumber,
        //                 this->xDbResRec.lResultID,
        //                 pcNextTest->bDeferred);
        // TLOG_DETAIL_1("%s", Sample.GetString()); 



        ///////////// Support macros to make logging easy. ///////////////
        // /// Macro to init static members. Use this once only in your module.
        // #define TLOG_INIT() \
        // CMN_CHAR CMN_CTraceLogger::mzTraceFlags[] = { 0 }; \
        // CMN_BOOL CMN_CTraceLogger::mbStdout = CMN_TRUE; \
        // CMN_BOOL CMN_CTraceLogger::mbTimingEnable = CMN_FALSE; \
        // CMN_BOOL CMN_CTraceLogger::mbFuncEnable = CMN_FALSE; \
        // CMN_CHAR CMN_CTraceLogger::mkErr = '*'; \
        // CMN_CHAR CMN_CTraceLogger::mkInfo = '-'; \
        // CMN_CHAR CMN_CTraceLogger::mkDetail = '~'; \
        // const CMN_CHAR* (*CMN_CTraceLogger::mRetStatFunc)(RET_STAT RetStat) = 0;

        // /// Macro for declaring a context logger. Tests exit condition of RET_STAT and record failure.
        // #define TLOG_CONTEXT(func, rs) CMN_CTraceLogger _logger(#func, &rs);
        // /// Macro for declaring a context logger with timing information.
        // #define TLOG_CONTEXT_T(func, rs) CMN_CTraceLogger _logger(#func, &rs, CMN_TRUE);
        // /// Macro for declaring a simple context logger with no RET_STAT checking.
        // #define TLOG_CONTEXT_S(func) CMN_CTraceLogger _logger(#func);

        // /// Macros for capturing general information.
        // #define TLOG_INFO_0(text) _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text);
        // #define TLOG_INFO_1(text, p1) _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1);
        // #define TLOG_INFO_2(text, p1, p2) _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1, p2);
        // #define TLOG_INFO_3(text, p1, p2, p3) _logger.Trace(CMN_CTraceLogger::mkInfo, __LINE__, text, p1, p2, p3);

        // #define TLOG_DETAIL_0(text) _logger.Trace(CMN_CTraceLogger::mkDetail, __LINE__, text);
        // #define TLOG_DETAIL_1(text, p1) _logger.Trace(CMN_CTraceLogger::mkDetail, __LINE__, text, p1);
        // #define TLOG_DETAIL_2(text, p1, p2) _logger.Trace(CMN_CTraceLogger::mkDetail, __LINE__, text, p1, p2);
        // #define TLOG_DETAIL_3(text, p1, p2, p3) _logger.Trace(CMN_CTraceLogger::mkDetail, __LINE__, text, p1, p2, p3);


    }


    ///// <summary>OLD Experimental class to log enter/exit scope.</summary>
    //public class Scoper : IDisposable
    //{
    //    readonly string _func;
    //    static readonly List<string> _captures = [];

    //    public static List<string> Captures { get { return _captures; } }

    //    [MethodImpl(MethodImplOptions.NoInlining)]
    //    public Scoper(string func = "???")
    //    {
    //        _func = func;
    //        _captures.Add($"{_func}: Scoper constructor");

    //        //var sf1 = new StackFrame(1);
    //        //var sf2 = new StackFrame(2);

    //        //var cm = sf1.GetMethod();
    //        //var scm = cm.ToString();

    //        //var st = new StackTrace(sf1);
    //        //var frm = st.GetFrame(0);
    //        //var sfrm = frm.ToString();
    //        //var cm = frm.GetMethod();
    //        //var scm = cm.ToString();
    //        //cm.CustomAttributes;
    //        //cm.Name;
    //        //cm.ReflectedType;

    //        //DumpStackFrame(sf1);

    //        // System.Diagnostics.attr
    //    }

    //    void DumpStackFrame(StackFrame frame)
    //    {
    //        MethodBase method = frame.GetMethod();

    //        MethodBody body = method.GetMethodBody();
    //        Console.WriteLine("Method: {0}", method.Name);

    //        var mparams = method.GetParameters();
    //        foreach (var paramInfo in mparams)
    //        {
    //            Console.WriteLine("    Param: {0}", paramInfo.ToString());
    //        }

    //        foreach (LocalVariableInfo variableInfo in body.LocalVariables)
    //        {
    //            Console.WriteLine("    Variable: {0}", variableInfo.ToString());

    //            foreach (PropertyInfo property in variableInfo.LocalType.GetProperties())
    //            {
    //                Console.WriteLine("        Property: {0}", property.Name);
    //            }
    //        }
    //    }

    //    void DumpStackFrames()
    //    {
    //        StackTrace trace = new StackTrace();
    //        foreach (StackFrame frame in trace.GetFrames())
    //        {
    //            MethodBase method = frame.GetMethod();
    //            MethodBody body = method.GetMethodBody();
    //            Console.WriteLine("Method: {0}", method.Name);

    //            foreach (LocalVariableInfo variableInfo in body.LocalVariables)
    //            {
    //                Console.WriteLine("\tVariable: {0}", variableInfo.ToString());

    //                foreach (PropertyInfo property in variableInfo.LocalType.GetProperties())
    //                {
    //                    Console.WriteLine("\t\tProperty: {0}", property.Name);
    //                }
    //            }
    //        }
    //    }

    //    ~Scoper()
    //    {
    //        _captures.Add($"{_func}: Scoper destructor");
    //    }

    //    public void Dispose()
    //    {
    //        _captures.Add($"{_func}: Scoper dispose");
    //    }

    //    //[MethodImpl(MethodImplOptions.NoInlining)]
    //    //public static MethodBase? GetMyCaller()
    //    //{
    //    //    var st = new StackTrace(new StackFrame(1));
    //    //    var cm = st.GetFrame(0).GetMethod();
    //    //    return cm;
    //    //}

    //}




    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LogMethodExecutionAttribute : Attribute
    {
        public string LogMessage { get; }

        public LogMethodExecutionAttribute(string logMessage)
        {
            LogMessage = logMessage;
        }
    }

    //public class LogMethodExecutionFilter : IActionFilter
    //{
    //    public void OnActionExecuting(ActionExecutingContext context)
    //    {
    //        var method = context.ActionDescriptor.MethodInfo;
    //        var logAttribute = method.GetCustomAttribute<LogMethodExecutionAttribute>();
    //        if (logAttribute != null)
    //        {
    //            // Your logging logic here
    //            Console.WriteLine($"Log: {logAttribute.LogMessage}");
    //        }
    //    }
    //    public void OnActionExecuted(ActionExecutedContext context)
    //    {
    //        // Post-action logic
    //    }
    //}





    public class Scoper_orig : IDisposable
    {
        readonly string _funcName;
        static readonly List<string> _captures = [];

        public static List<string> Captures { get { return _captures; } }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Scoper_orig(string funcName)
        {
            _funcName = funcName;
            _captures.Add($"{_funcName}: Scoper constructor");

            var sf1 = new StackFrame(1);
            var sf2 = new StackFrame(2);

            var cm = sf1.GetMethod();
            var scm = cm.ToString();

            //var st = new StackTrace(sf1);
            //var frm = st.GetFrame(0);
            //var sfrm = frm.ToString();
            //var cm = frm.GetMethod();
            //var scm = cm.ToString();
            //cm.CustomAttributes;
            //cm.Name;
            //cm.ReflectedType;

            DumpStackFrame(sf1);

            // System.Diagnostics.attr
        }

        void DumpStackFrame(StackFrame frame)
        {
            MethodBase method = frame.GetMethod();

            MethodBody body = method.GetMethodBody();
            Console.WriteLine("Method: {0}", method.Name);

            var mparams = method.GetParameters();
            foreach (var paramInfo in mparams)
            {
                Console.WriteLine("    Param: {0}", paramInfo.ToString());
            }

            foreach (LocalVariableInfo variableInfo in body.LocalVariables)
            {
                Console.WriteLine("    Variable: {0}", variableInfo.ToString());

                foreach (PropertyInfo property in variableInfo.LocalType.GetProperties())
                {
                    Console.WriteLine("        Property: {0}", property.Name);
                }

            }
        }

        void DumpStackFrames()
        {
            StackTrace trace = new StackTrace();
            foreach (StackFrame frame in trace.GetFrames())
            {
                MethodBase method = frame.GetMethod();
                MethodBody body = method.GetMethodBody();
                Console.WriteLine("Method: {0}", method.Name);


                foreach (LocalVariableInfo variableInfo in body.LocalVariables)
                {
                    Console.WriteLine("\tVariable: {0}", variableInfo.ToString());

                    foreach (PropertyInfo property in variableInfo.LocalType.GetProperties())
                    {
                        Console.WriteLine("\t\tProperty: {0}", property.Name);
                    }

                }

            }
        }

        ~Scoper_orig()
        {
            _captures.Add($"{_funcName}: Scoper destructor");
        }

        public void Dispose()
        {
            _captures.Add($"{_funcName}: Scoper dispose");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static MethodBase? GetMyCaller()
        {
            var st = new StackTrace(new StackFrame(1));
            var cm = st.GetFrame(0).GetMethod();
            return cm;
        }

    }

}


/* tracer.py
import sys
import os
import time
import traceback
import functools
import platform
import inspect



#-----------------------------------------------------------------------------------
#---------------------------- Private fields ---------------------------------------
#-----------------------------------------------------------------------------------

# The trace file.
_ftrace = None

# For elapsed time stamps.
_trace_start_time = 0

# Dynamic flag controls execution.
_trace_enabled = False

# If false continue to execute the steps after error.
_stop_on_exception = False

# Arg separators for records.
_sep = ('(', ')')  # or ('[', ']') ('|', '|')


#-----------------------------------------------------------------------------------
#---------------------------- Public trace functions -------------------------------
#-----------------------------------------------------------------------------------

#---------------------------------------------------------------------------
def start(trace_fn, clean_file=True, stop_on_exception=True, sep=('(', ')')):
    '''
    - clean_file: cleans the file (default is True)
    - stop_on_exception: If false continue to execute the steps after error.
    - sep: Arg separators for records. '[', ']', '|',...
    '''
    global _ftrace
    global _trace_start_time
    global _trace_enabled
    global _stop_on_exception

    stop()  # just in case

    if clean_file:
        try:
            os.remove(trace_fn)
        except:
            pass    

    # Open file now and keep it open. Open/close on every write is too expensive.
    # Note that each instance requires its own file.
    try:
        _ftrace = open(trace_fn, 'a')
        _trace_start_time = time.perf_counter_ns()
        _trace_enabled = True
    except Exception as e:
        _ftrace = None
        _trace_start_time = 0
        _trace_enabled = False
        raise RuntimeError(f'Failed to open trace file - disabling tracing. {e}')


#---------------------------------------------------------------------------
def stop():
    '''Stop tracing.'''
    global _ftrace
    global _trace_enabled

    if _ftrace is not None:
        _ftrace.flush()
        _ftrace.close()
        _ftrace = None
    _trace_enabled = False


#---------------------------------------------------------------------------
def enable(enable):
    '''Gate tracing without start/stop.'''
    global _trace_enabled
    _trace_enabled = enable


#---------------------------------------------------------------------------
def T(*args):
    '''General purpose trace function for user code.'''
    if _ftrace is not None and _trace_enabled:
        func_name, line = _get_caller_site(2)
        argl = []
        for m in args:
            argl.append(m)
        _trace(func_name, line, argl)


#---------------------------------------------------------------------------
def A(cond):
    '''General purpose assert function for user code.'''
    if _ftrace is not None and _trace_enabled and not cond:
        func_name, line = _get_caller_site(2)
        site = f'{func_name}:{line}'
        raise AssertionError(site)


#---------------------------------------------------------------------------
def trfunc(f):
    '''Decorator to support function entry/exit tracing.'''
    @functools.wraps(f)
    def wrapper(*args, **kwargs):
        global _trace_enabled
        res = None

        # Check for enabled.
        if _ftrace is not None and _trace_enabled:
            # Instrumented execution.
            msgs = []
            if len(args) > 0:
                for i in range(len(args)):
                    msgs.append(f'{args[i]}') # nice to have name but difficult
            if len(kwargs) > 0:
                for k,v in kwargs.items():
                    msgs.append(f'{k}:{v}')

            func_name = _get_func_name_from_func(f)

            # Record entry.
            _trace(func_name, 'enter', msgs)

            # Execute the wrapped function.
            ret = []
            try:
                res = f(*args, **kwargs)

                # No runtime errors.
                ret.append(f'{res}')
                # Record exit.
                _trace(func_name, 'exit', ret)

            except AssertionError as e:
                # User A() hit. Record a useful message.
                _trace(e, 'assert')
                if _stop_on_exception:
                    # Stop execution now.
                    _trace_enabled = False

            except Exception as e:
                # Other exception in T() code. Record a useful message.
                tb = e.__traceback__
                frame = traceback.extract_tb(tb)[-1]
                _trace(_get_func_name_from_frame(frame), frame.lineno, [f'exception: {e}'])
                if _stop_on_exception:
                    # Stop execution now.
                    _trace_enabled = False

        else:
            # Simple execution.
            res = f(*args, **kwargs)

        return res

    return wrapper


#-----------------------------------------------------------------------------------
#---------------------------- Private functions ------------------------------------
#-----------------------------------------------------------------------------------


#---------------------------------------------------------------------------
def _trace(func_name, line, args=None):
    '''Do one trace record.'''
    elapsed = time.perf_counter_ns() - _trace_start_time
    msec = elapsed // 1000000
    usec = elapsed // 1000

    parts = []
    parts.append(f'{msec:04}.{usec:03}')
    parts.append(f'{func_name}:{line}')

    if args is not None:
        for a in args:
            parts.append(f'{_sep[0]}{a}{_sep[1]}')
    s = ' '.join(parts) + '\n'

    # Write the record.
    _ftrace.write(s)


#---------------------------------------------------------------------------
def _get_caller_site(stkpos):
    '''Dig out caller source func name and line from call stack. Includes class name if member.'''
    frame = sys._getframe(stkpos)
    if 'self' in frame.f_locals:
        class_name = frame.f_locals['self'].__class__.__name__
        func_name = f'{class_name}.{frame.f_code.co_name}'
    else:
        func_name = frame.f_code.co_name  # could also be '<module>'
    return (func_name, frame.f_lineno)


#---------------------------------------------------------------------------
def _get_func_name_from_func(f):
    '''Dig out func name from function object.'''
    func_name = getattr(f, '__qualname__')
    return func_name


#---------------------------------------------------------------------------
def _get_func_name_from_frame(frame):
    '''Dig out func name and line from frame. Includes class name if member.'''
    if frame.locals is not None and 'self' in frame.locals:
        class_name = frame.locals['self'].__class__.__name__
        func_name = f'{class_name}.{frame.name}'
    else:
        func_name = frame.name  # could also be '<module>'
    return func_name




import sys
import os
import datetime
import importlib
import unittest
import utils

# Add source path to sys.path.
my_dir = os.path.dirname(__file__)
utils.ensure_import(my_dir, '..')
# OK to import now.
import tracer as tr
# Benign reload in case it's edited.
importlib.reload(tr)


# Some optional shorthand.
trfunc = tr.trfunc
A = tr.A
T = tr.T


#------------------------ Dummy test object ------------------------------------------
class ExampleClass(object):
    ''' Class function tracing.'''

    # Note: don't use @trfunc on constructor!
    def __init__(self, name, tags, arg):
        '''Construction.'''
        T('making one ExampleClass', name, tags, arg)
        self._name = name
        self._tags = tags
        self._arg = arg

    @trfunc
    def do_something(self, arg):
        '''Entry/exit is traced with args and return value.'''
        res = f'{self._arg}-user-{arg}'
        return res

    @trfunc
    def do_class_assert(self, arg):
        '''Entry/exit is traced with args and return value.'''
        A(1 == 2)

    @trfunc
    def do_class_exception(self, arg):
        '''Entry/exit is traced with args and return value.'''
        x = 1 / 0

    def __str__(self):
        '''Required for readable trace.'''
        s = f'ExampleClass:{self._name} tags:{self._tags} arg:{self._arg}'
        return s


#------------------------ Test functions ------------------------------------------
def no_trfunc_function(s):
    '''Entry/exit is not traced but explicit traces are ok.'''
    T(f'I still can do this => "{s}"')

@trfunc
def another_function(a_list, a_dict):
    '''Entry/exit is traced with args and return value.'''
    return len(a_list) + len(a_dict)

@trfunc
def one_function(a1: int, a2: float):
    '''Entry/exit is traced with args and return value.'''
    cl1 = ExampleClass('number 1', [45, 78, 23], a1)
    cl2 = ExampleClass('number 2', [100, 101, 102], a2)
    T(cl1)
    T(cl2)
    ret = f'answer is cl1:{cl1.do_something(a1)}...cl2:{cl2.do_something(a2)}'

    ret = f'{cl1.do_class_assert(a1)}'

    ret = f'{cl1.do_class_exception(a2)}'
    return ret

@trfunc
def exception_function():
    '''Cause exception and handling.'''
    i = 0
    return 1 / i

@trfunc
def assert_function():
    '''Assert processing.'''
    i = 10
    j = 10

    A(i == j)  # ok - no trace
    i += 1
    A(i == j)  # assert

@trfunc
def do_a_suite(alpha, number):
    '''Make a nice suite with entry/exit and return value.'''
    T('something sweet')

    ret = one_function(5, 9.126)

    exception_function()

    assert_function()

    no_trfunc_function('can you see me?')
    # ret = another_function([33, 'tyu', 3.56], {'aaa': 111, 'bbb': 222, 'ccc': 333})
    ret = another_function([33, 'tyu', 3.56], {'aaa': number, alpha: 222, 'ccc': 333})
    return ret


#-----------------------------------------------------------------------------------
class TestTracer(unittest.TestCase):

    def setUp(self):
        pass

    def tearDown(self):
        pass

    def test_success(self):
        trace_fn = os.path.join(my_dir, 'out', 'tracer.log')
        tr.start(trace_fn, clean_file=True, stop_on_exception=True, sep=('(', ')'))

        T(f'Start {do_a_suite.__name__}:{do_a_suite.__doc__} {datetime.datetime.now()}')
        do_a_suite(number=911, alpha='abcd')  # named args
        tr.stop()  # Always clean up resources!!

        # Examine generated contents.
        lines = []
        with open(trace_fn) as f:
            lines = f.readlines()

        self.assertEqual(len(lines), 25)


*/