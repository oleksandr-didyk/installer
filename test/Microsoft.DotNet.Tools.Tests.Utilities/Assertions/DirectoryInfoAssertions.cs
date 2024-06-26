﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.DotNet.Tools.Test.Utilities
{
    public class DirectoryInfoAssertions
    {
        private DirectoryInfo _dirInfo;

        public DirectoryInfoAssertions(DirectoryInfo dir)
        {
            _dirInfo = dir;
        }

        public DirectoryInfo DirectoryInfo => _dirInfo;

        public AndConstraint<DirectoryInfoAssertions> Exist()
        {
            Execute.Assertion.ForCondition(_dirInfo.Exists)
                .FailWith("Expected directory {0} does not exist.", _dirInfo.FullName);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveFile(string expectedFile, string because = "", params object[] reasonArgs)
        {
            var file = _dirInfo.EnumerateFiles(expectedFile, SearchOption.TopDirectoryOnly).SingleOrDefault();

            Execute.Assertion
                .ForCondition(file != null)
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected File {expectedFile} cannot be found in directory {_dirInfo.FullName}.");

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveTextFile(
            string expectedFile,
            string expectedContents,
            string because = "",
            params object[] reasonArgs)
        {
            this.HaveFile(expectedFile, because, reasonArgs);

            var file = _dirInfo.EnumerateFiles(expectedFile, SearchOption.TopDirectoryOnly).SingleOrDefault();

            var contents = File.ReadAllText(file.FullName);

            Execute.Assertion
                .ForCondition(contents.Equals(expectedContents))
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected file {expectedFile} to contain \n\n{expectedContents}\n\nbut it contains\n\n{contents}\n");

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotHaveFile(
            string expectedFile, 
            string because = "", 
            params object [] reasonArgs)
        {
            var file = _dirInfo.EnumerateFiles(expectedFile, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Execute.Assertion
                .ForCondition(file == null)
                .BecauseOf(because, reasonArgs)
                .FailWith("File {0} should not be found in directory {1}.", expectedFile, _dirInfo.FullName);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveFiles(IEnumerable<string> expectedFiles)
        {
            foreach (var expectedFile in expectedFiles)
            {
                HaveFile(expectedFile);
            }

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveTextFiles(
            IDictionary<string, string> expectedFiles, 
            string because = "", 
            params object[] reasonArgs)
        {
            foreach (var expectedFile in expectedFiles)
            {
                HaveTextFile(expectedFile.Key, expectedFile.Value, because, reasonArgs);
            }

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveFilesMatching(
            string expectedFilesSearchPattern, 
            SearchOption searchOption, 
            string because = "", 
            params object[] reasonArgs)
        {
            var matchingFileExists = _dirInfo.EnumerateFiles(expectedFilesSearchPattern, searchOption).Any();

            Execute.Assertion
                .ForCondition(matchingFileExists == true)
                .BecauseOf(because, reasonArgs)
                .FailWith("Expected directory {0} to contain files matching {1}, but no matching file exists.",
                    _dirInfo.FullName, expectedFilesSearchPattern);

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotHaveFiles(
            IEnumerable<string> expectedFiles, 
            string because = "", 
            params object [] reasonArgs)
        {
            foreach (var expectedFile in expectedFiles)
            {
                NotHaveFile(expectedFile, because, reasonArgs);
            }

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotHaveFilesMatching(string expectedFilesSearchPattern, SearchOption searchOption)
        {
            var matchingFileCount = _dirInfo.EnumerateFiles(expectedFilesSearchPattern, searchOption).Count();
            Execute.Assertion.ForCondition(matchingFileCount == 0)
                .FailWith("Found {0} files that should not exist in directory {1}. No file matching {2} should exist.",
                    matchingFileCount, _dirInfo.FullName, expectedFilesSearchPattern);
            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> HaveDirectory(string expectedDir)
        {
            var dir = _dirInfo.EnumerateDirectories(expectedDir, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Execute.Assertion.ForCondition(dir != null)
                .FailWith("Expected directory {0} cannot be found inside directory {1}.", expectedDir, _dirInfo.FullName);

            return new AndConstraint<DirectoryInfoAssertions>(new DirectoryInfoAssertions(dir));
        }

        public AndConstraint<DirectoryInfoAssertions> HaveDirectories(IEnumerable<string> expectedDirs)
        {
            foreach (var expectedDir in expectedDirs)
            {
                HaveDirectory(expectedDir);
            }

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotHaveDirectory(string unexpectedDir)
        {
            var dir = _dirInfo.EnumerateDirectories(unexpectedDir, SearchOption.TopDirectoryOnly).SingleOrDefault();
            Execute.Assertion.ForCondition(dir == null)
                .FailWith("Directory {0} should not be found in directory {1}.", unexpectedDir, _dirInfo.FullName);

            return new AndConstraint<DirectoryInfoAssertions>(new DirectoryInfoAssertions(dir));
        }

        public AndConstraint<DirectoryInfoAssertions> NotHaveDirectories(IEnumerable<string> unexpectedDirs)
        {
            foreach (var unexpectedDir in unexpectedDirs)
            {
                NotHaveDirectory(unexpectedDir);
            }

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> OnlyHaveFiles(IEnumerable<string> expectedFiles)
        {
            var actualFiles = _dirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Select(f => f.Name);
            var missingFiles = Enumerable.Except(expectedFiles, actualFiles);
            var extraFiles = Enumerable.Except(actualFiles, expectedFiles);
            var nl = Environment.NewLine;

            Execute.Assertion.ForCondition(!missingFiles.Any())
                .FailWith($"Following files cannot be found inside directory {_dirInfo.FullName} {nl} {string.Join(nl, missingFiles)}");

            Execute.Assertion.ForCondition(!extraFiles.Any())
                .FailWith($"Following extra files are found inside directory {_dirInfo.FullName} {nl} {string.Join(nl, extraFiles)}");

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> BeEmpty()
        {
            Execute.Assertion.ForCondition(!_dirInfo.EnumerateFileSystemInfos().Any())
                .FailWith($"The directory {_dirInfo.FullName} is not empty.");

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotBeEmpty()
        {
            Execute.Assertion.ForCondition(_dirInfo.EnumerateFileSystemInfos().Any())
                .FailWith($"The directory {_dirInfo.FullName} is empty.");

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }

        public AndConstraint<DirectoryInfoAssertions> NotExist(string because = "", params object[] reasonArgs)
        {
            Execute.Assertion
                .ForCondition(_dirInfo.Exists == false)
                .BecauseOf(because, reasonArgs)
                .FailWith($"Expected directory {_dirInfo.FullName} to not exist, but it does.");

            return new AndConstraint<DirectoryInfoAssertions>(this);
        }
    }
}
