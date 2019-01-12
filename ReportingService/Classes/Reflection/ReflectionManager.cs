﻿using System;

using System.Reflection;
using ReportingServiceDatabase.Classes.Exceptions;
using ReportingServiceDatabase.Classes.Database;
using ReportingServiceDatabase.DataSets;
using ReportingServiceDatabase.Logging;


namespace ReportingService.Classes.Reflection
{
    public class ReflectionManager
    {
        public ReflectionManager()
        {
        }

        public static Assembly LoadAssembly(String fileName)
        {
            Logger.Debug("Loading assembly: " + fileName);

            return Assembly.LoadFile(fileName);
        }

        public static void ProcessEvent(Assembly asm, String className, String methodName, ReportEvent evt, ConnectionManager connectionManager, int sleepTime)
        {
            Type type = asm.GetType(className);

            if (type == null)
            {
                throw new EventProcessorClassNotFound();
            }

            ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
            object calc = ctor.Invoke(null);
            MethodInfo m = type.GetMethod(methodName);

            if (m == null)
            {
                throw new EventProcessorClassEntryNotImplemented();
            }

            object[] args = new object[3];

            args[0] = evt;
            args[1] = connectionManager;
            args[2] = sleepTime;

            m.Invoke(calc, args);
        }
    }
}
