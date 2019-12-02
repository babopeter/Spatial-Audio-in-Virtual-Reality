/*
	Created by Carl Emil Carlsen.
	Copyright 2018 Sixth Sensor.
	All rights reserved.
	http://sixthsensor.dk
*/

namespace OscSimpl
{
	using System.Text;
	using System.Linq;

	/// <summary>
	/// Helper class for dealing with OSC addresses.
	/// </summary>
	public static class OscAddress
	{
		const char addressPrefix = '/';

		// Supported pattern matching wildcards.
		const char anySingleWildcard = '?';
		const char anyMultiWildcard = '*';
		const char charGroupBeginWildcard = '[';
		const char charGroupEndWildcard = ']';
		const char charGroupRangeWildcard = '-';
		const char charGroupNegationWildcard = '!';
		const char stringListBeginWildcard = '{';
		const char stringListEndWildcard = '}';
		const char stringListSeperatorWildcard = ',';

		// Other.
		static StringBuilder _sb = new StringBuilder();
		const string doubleAddresPrefix = "//"; // XPath-style wildcard was introduced in OSC 1.1 but is not supported here.
		static readonly char[] specialPatternCharacters = {
			anySingleWildcard,
			anyMultiWildcard,
			charGroupBeginWildcard,
			charGroupEndWildcard,
			charGroupRangeWildcard,
			charGroupNegationWildcard,
			stringListBeginWildcard,
			stringListEndWildcard,
			stringListSeperatorWildcard
		};


		public static void Sanitize( ref string address )
		{
			bool zeroLength = address.Length == 0;
			bool missingAddressPrefix = !zeroLength && address[0] != addressPrefix;
			bool unsupportedDoublePrefixes = !zeroLength && address.Contains( doubleAddresPrefix );

			if( zeroLength || missingAddressPrefix || unsupportedDoublePrefixes )
			{
				_sb.Length = 0;
				if( zeroLength || missingAddressPrefix ) _sb.Append( addressPrefix );
				if( unsupportedDoublePrefixes ){
					bool isPrevAddressPrefix = false;
					int i = 0;
					while( i < address.Length ){
						char c = address[i];
						bool isAddressPrefix = c == addressPrefix;
						if( !(isAddressPrefix && isPrevAddressPrefix) ) _sb.Append( c );
						isPrevAddressPrefix = isAddressPrefix;
					}
				} else {
					_sb.Append( address );
				}

				address = _sb.ToString();
			}
		}


		/// <summary>
		/// Checks wheter the string has any of the supported specual pattern matching characters.
		/// </summary>
		public static bool HasAnySpecialPatternCharacter( string address )
		{
			foreach( char c in specialPatternCharacters ) if( address.Contains( c ) ) return true;
			return false;
		}



		/// <summary>
		/// Evaluates whether address1 matches address2 following OSC 1.0 Address Pattern Matching.
		/// In the original 1.0 specification matching is only one-way, this method is two-way.
		/// 1.0 one-way matching only allows messages to contain special Address Patterns, presuming
		/// the messages know where to go. OSC simpl also allows OscMappings (OSC Methods) to 
		/// contain special Address Patterns.
		/// </summary>
		public static bool IsMatching( string address1, string address2 )
		{
			// Char indexes for address1 and address2.
			int c1 = 0;
			int c2 = 0;

			// Validate that addresses start with prefix.
			if( address1[c1++] != addressPrefix ) return false;
			if( address2[c2++] != addressPrefix ) return false;

			// Loop through all characters.
			char ch1;
			char ch2;
			while( c1 < address1.Length && c2 < address2.Length )
			{
				ch1 = address1[c1++];
				ch2 = address2[c2++];

				//Debug.Log( meCh  + " AND " + maCh );

				// RULE 1: Any-single-wildcard, skip char.
				if( ch1 == anySingleWildcard || ch2 == anySingleWildcard ) continue;  // Next two chars.

				// RULE 2: Any-multi-wildcard, skip address "part". 
				if( ch1 == anyMultiWildcard || ch2 == anyMultiWildcard ) {
					while( c1 < address1.Length ) if( address1[c1++] == addressPrefix ) break;
					while( c2 < address2.Length ) if( address2[c2++] == addressPrefix ) break;
					continue; // Next two chars.
				}

				// RULE 3: Char-group-wildcard.
				bool ch1IsBegin = ch1 == charGroupBeginWildcard;
				bool ch2IsBegin = ch2 == charGroupBeginWildcard;
				if( ch1IsBegin || ch2IsBegin ) {
					// Two colliding char groups is not supported.
					if( ch1IsBegin && ch2IsBegin ) return false;

					if( ch1IsBegin ) {
						if( IsMatchingCharGroup( ch2, address1, ref c1 ) ) continue; // Next two chars.
						return false; // No match
					}

					// maHasGroup
					if( IsMatchingCharGroup( ch1, address2, ref c2 ) ) continue; // Next two chars.
					return false; // No match
				}

				// RULE 4: String-lists.wildcard.
				ch1IsBegin = ch1 == stringListBeginWildcard;
				ch2IsBegin = ch2 == stringListBeginWildcard;
				if( ch1IsBegin || ch2IsBegin ) {
					// Two colliding string lists is not supported.
					if( ch1IsBegin && ch2IsBegin ) return false;

					if( ch1IsBegin ) {
						c2--;
						if( IsMatchingStringList( address2, ref c2, address1, ref c1 ) ) continue; // Next two chars.
						return false; // No match
					}

					// maHasGroup
					c1--;
					if( IsMatchingStringList( address1, ref c1, address2, ref c2 ) ) continue; // Next two chars.
					return false; // No match
				}

				// RULE 5: Phew! Just compare.
				if( ch1 != ch2 ) return false;
			}

			//Debug.Log( me + " == " + address1.Length + " AND " + ma + " == " + address2.Length );
			return c1 == address1.Length && c2 == address2.Length;
		}


		/// <summary>
		/// Returns true only if testChar is matching charGroup AND if charGroup is valid.
		/// Index must be at first position in group.
		/// </summary>
		static bool IsMatchingCharGroup( char testChar, string charGroup, ref int index )
		{
			//Debug.Log( "IsMatchingCharGroup. testChar: " + testChar + ", charGroup: " + charGroup + ", index: " + index );

			// Validate index.
			if( index >= charGroup.Length ) return false;

			// Check for valid index and negation.
			bool isNegated = false;
			if( charGroup[index] == charGroupNegationWildcard ) {
				isNegated = true;
				index++;
				// Validate index again.
				if( index >= charGroup.Length ) return false;
			}

			// Read first group char.
			char ch = charGroup[index];

			// Check for group without content.
			if( ch == charGroupEndWildcard ) return false;

			// Check range case.
			if( index+3 < charGroup.Length && charGroup[index+1] == charGroupRangeWildcard && charGroup[index+3] == charGroupEndWildcard ) {
				if( (testChar < ch || testChar > charGroup[index+2]) == isNegated ) {
					index += 4;
					return true;
				}
				return false;
			}

			// Check regular compare case.
			bool isMatch = false;
			while( index < charGroup.Length ) {
				ch = charGroup[index++];
				if( ch == charGroupEndWildcard ) break;
				if( ch == addressPrefix ) return false; // Invalid because no closing of group.
				if( !isMatch && ch == testChar ) isMatch = true;
			}
			if( index == charGroup.Length && ch != charGroupEndWildcard ) return false; // Invalid because no closing of group.

			return isMatch == !isNegated;
		}



		/// <summary>
		/// Returns true only if testString is matching stringList AND if stringList is valid.
		/// Index must be at first position in group.
		/// </summary>
		static bool IsMatchingStringList( string testString, ref int testIndex, string stringList, ref int listIndex )
		{
			// Validate index.
			if( listIndex >= stringList.Length ) return false;

			int tempTestIndex = testIndex;
			int listBeginIndex = listIndex;
			bool isMatch = true;
			while( listIndex < stringList.Length )
			{
				char ch = stringList[listIndex++];

				// If end of list.
				if( ch == stringListEndWildcard ){
					// If isMatch is still true and we actually read something, then found match!
					if( isMatch && tempTestIndex != testIndex ){
						testIndex = tempTestIndex;
						while( listIndex < stringList.Length ) if( stringList[listIndex++] == stringListEndWildcard ) return true;
						return true;
					}
					return false;
				}

				// If string seperator.
				if( ch == stringListSeperatorWildcard ){
					// If isMatch is still true and we actually read something, then found match!
					if( isMatch && listIndex != listBeginIndex ) {
						testIndex = tempTestIndex;
						while( listIndex < stringList.Length ) if( stringList[listIndex++] == stringListEndWildcard ) return true;
						return false;
					}
					// Else, reset test string index and continue to next string.
					tempTestIndex = testIndex;
					isMatch = true; // New hope.
					continue;
				}

				// Check test string bounds. We do it here, because it may have been reset.
				if( tempTestIndex >= testString.Length ) return false;

				// Check current char if there is still hope for current string.
				if( isMatch && ch != testString[tempTestIndex] ) isMatch = false; // No match for current string.

				// Next test char.
				tempTestIndex++;
			}

			// No match.
			return false;
		}
	}
}