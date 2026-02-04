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
using static NLab.Utils;


// lots from - https://markheath.net/post/starting-threads-in-dotnet


namespace NLab
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LogMethodExecutionAttribute : Attribute
    {
        public string LogMessage { get; }

        public LogMethodExecutionAttribute(string logMessage)
        {
            LogMessage = logMessage;
        }
    }

    /// <summary>TODO Experimental class to log enter/exit scope.</summary>
    //TODO Use attribute? https://medium.com/@nwonahr/creating-and-using-custom-attributes-in-c-for-asp-net-core-c4f7d3db1829
    public class Scoper : IDisposable
    {
        readonly string _id;
        static readonly List<string> _captures = [];

        public static List<string> Captures { get { return _captures; } }

        public Scoper(string id)
        {
            _id = id;
            // _captures.Add($"{_id}: Scoper.Scoper(string id)");
            Console.WriteLine($"Scoper.Scoper({id})");
        }

        public void Dispose()
        {
            // _captures.Add($"{_id}: Scoper.Dispose()");
            Console.WriteLine($"Scoper.Dispose() [{_id}]");
        }


        void DoLogMethodApp()
        {
            Tell(NON, "Calling TestMethod() 1");

            RunTestMethod("here we go");

            Tell(NON, "Calling TestMethod() 2");

            RunTestMethod("try again");

            Tell(NON, "Finished TestMethod()");

            //AsyncPlay ap = new();
            //ap.Notif += (object? sender, NotifEventArgs e) => Tell(INF, e.Message);
            //ap.Go();
        }

        [LogMethodExecution("testing 123")]
        void RunTestMethod(string s)
        {
            Tell(NON, $"App.RunTestMethod({s}) entry");
            using var sc = new Scoper("1-1-1-1");
            int l = s.Length;
            Tell(INF, $"string is {l} long");
            Tell(NON, $"App.RunTestMethod() exit");
        }
    }
}


/*
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