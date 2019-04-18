using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Juno.Etc;
using Juno.Interfaces;
using Juno.Services;

namespace Juno
{
    public class FunctionDetour : IDetour
    {
        private const BindingFlags BINDING_FLAGS = BindingFlags.FlattenHierarchy | 
            BindingFlags.Instance | 
            BindingFlags.NonPublic | 
            BindingFlags.Public | 
            BindingFlags.Static;

        private byte[] _detourBytes;
        
        private byte[] _originalBytes;
        
        private IntPtr _originalFunctionAddress;

        public FunctionDetour(MethodInfo sourceFunction, MethodInfo targetFunction)
        {
            if (sourceFunction == null)
            {
                throw new ArgumentNullException($"The parameter '{nameof(sourceFunction)}' can't be null!");
            }

            if (targetFunction == null)
            {
                throw new ArgumentNullException($"The parameter '{nameof(targetFunction)}' can't be null!");
            }

            InitialiseDetour(sourceFunction, targetFunction);
        }

        public FunctionDetour(IReflect sourceClassType, string sourceFuncName, IReflect targetClassType, string targetFuncName = "")
        {
            // Ensure the arguments passed in are valid

            if (string.IsNullOrWhiteSpace(sourceFuncName))
            {
                throw new ArgumentException($"The parameter '{nameof(sourceFuncName)}' can't be null / empty or whitespace!");
            }

            if (string.IsNullOrWhiteSpace(targetFuncName))
            {
                targetFuncName = sourceFuncName;
            }

            if (sourceClassType == null)
            {
                throw new ArgumentNullException($"The parameter '{nameof(sourceClassType)}' can't be null!");
            }

            if (targetClassType == null)
            {
                throw new ArgumentNullException($"The parameter '{nameof(targetClassType)}' can't be null!");
            }

            // Get the information about the original function

            var sourceFunc = sourceClassType.GetMethod(sourceFuncName, BINDING_FLAGS);

            if (sourceFunc == null)
            {
                throw new InvalidOperationException($"Could not find function {sourceFuncName} in class {sourceClassType}");
            }

            // Get the information about the target function

            var targetFunc = targetClassType.GetMethod(targetFuncName, BINDING_FLAGS);

            if (targetFunc == null)
            {
                throw new InvalidOperationException($"Could not find function {targetFuncName} in class {targetClassType}");
            }

            InitialiseDetour(sourceFunc, targetFunc);
        }

        private void InitialiseDetour(MethodInfo originalFunctionInfo, MethodInfo targetFunctionInfo)
        {
            // Ensure the operating system is valid

            ValidateOperatingSystem.Validate();

            // Ensure the functions are JIT compiled

            RuntimeHelpers.PrepareMethod(originalFunctionInfo.MethodHandle);

            RuntimeHelpers.PrepareMethod(targetFunctionInfo.MethodHandle);

            // Get a pointer to the original function

            _originalFunctionAddress = originalFunctionInfo.MethodHandle.GetFunctionPointer();

            // Get a pointer to the target function

            var targetFunctionAddress = targetFunctionInfo.MethodHandle.GetFunctionPointer();

            // Create shellcode to perform a function detour

            var shellcode = Environment.Is64BitProcess ? Shellcode.JumpToFunctionX64(targetFunctionAddress) : Shellcode.JumpToFunctionX86(targetFunctionAddress);

            // Save the bytes of the original function

            _originalBytes = new byte[shellcode.Length];

            Marshal.Copy(_originalFunctionAddress, _originalBytes, 0, shellcode.Length);

            // Save the bytes used to detour the original function to the target function

            _detourBytes = shellcode;
        }

        public void AddDetour()
        {
            // Write the detour bytes to the start of the original function
            
            Tools.InternalWriteMemory(_originalFunctionAddress, _detourBytes);
        }
        
        public void RemoveDetour()
        {
            // Write the original bytes to the start of the original function
            
            Tools.InternalWriteMemory(_originalFunctionAddress, _originalBytes);
        }
    }
}