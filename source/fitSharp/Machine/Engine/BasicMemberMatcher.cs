// Copyright � 2012 Syterra Software Inc. All rights reserved.
// The use and distribution terms for this software are covered by the Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file license.txt at the root of this distribution. By using this software in any fashion, you are agreeing
// to be bound by the terms of this license. You must not remove this notice, or any other, from this software.

using System;
using System.Collections.Generic;
using System.Reflection;
using fitSharp.Machine.Model;

namespace fitSharp.Machine.Engine {
    public class BasicMemberMatcher: MemberMatcher {
        public BasicMemberMatcher(object instance, MemberName memberName, MemberSpecification specification) {
            this.instance = instance;
            this.memberName = memberName;
            this.specification = specification;
        }

        public Maybe<RuntimeMember> Match(IEnumerable<MemberInfo> members) {
            foreach (var memberInfo in members) {
                var runtimeMemberFactory = RuntimeMemberFactory.MakeFactory(memberName, memberInfo);
                if (!runtimeMemberFactory.Matches(memberName)) continue;
                var runtimeMember = runtimeMemberFactory.MakeMember(instance);
                if (!Matches(runtimeMember)) continue;
                return new Maybe<RuntimeMember>(runtimeMember);
            }
            return Maybe<RuntimeMember>.Nothing;
        }

        bool Matches(RuntimeMember runtimeMember) {
            return
                specification.MatchesParameterCount(runtimeMember) &&
                specification.MatchesParameterTypes(runtimeMember) &&
                specification.MatchesParameterNames(runtimeMember);
        }

        readonly object instance;
        readonly MemberName memberName;
        readonly MemberSpecification specification;
    }
}
