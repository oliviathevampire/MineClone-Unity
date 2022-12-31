using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using Object = System.Object;

/**
 * An identifier used to identify things. This is also known as "resource location",
 * "namespaced ID", "location", or just "ID". This is a non-typed immutable object,
 * and identifies things using a combination of namespace and path. Identifiers should
 * always be compared using {@link #equals} method, not {@code ==}.
 * 
 * <h2 id="format">Format</h2>
 * <p>Identifiers are formatted as {@code <namespace>:<path>}. If the namespace and colon
 * are omitted, the namespace defaults to {@value #DEFAULT_NAMESPACE}.
 * 
 * <p><strong>The namespace and path must contain only ASCII lowercase letters ({@code
 * [a-z]}), ASCII digits ({@code [0-9]}), or the characters {@code _}, {@code .}, and
 * {@code -}. </strong> The path can also contain the standard path separator {@code
 * /}. Uppercase letters cannot be used. {@link #isValid} can be used to check whether a
 * string is a valid identifier. When handling externally provided identifiers, it should
 * either validate or use {@link #tryParse} instead of the constructor. Another common
 * mistake is using a formatted string with {@code %d} or {@code %f} to construct an
 * identifier without specifying the locate explicitly, as they are not guaranteed to be
 * ASCII digits in certain locales. Use {@link String#format(Locale, String, Object[])}
 * with {@link java.util.Locale#ROOT} instead of {@link String#formatted}.
 * 
 * <h3 id="namespace">Namespace</h3>
 * <p>The <strong>namespace</strong> of an identifier identifies the origin of the thing.
 * For example, two mods to the game could both add an item with the ID "orange";
 * the namespace is used to differentiate the two. (The convention is to use the ID
 * assigned to the mod as the namespace.)
 * 
 * <p>A namespace only determines the source of an identifier, and does not determine its purpose; so long as
 * two identifiers are used for different purposes, they can share the namespace and path.
 * For example, the identifier {@code minecraft:dirt} is shared by blocks and items.
 * There is no need to change the identifier to, say, {@code minecraft_block:dirt} or
 * {@code minecraft_item:dirt}.
 * 
 * <p>Several namespaces are reserved for vanilla use. While those identifiers can be used for
 * referencing and overwriting vanilla things, it is highly discouraged to use them to
 * identify your own, new things. For example, a modded block or a new biome added by
 * data packs should not use the reserved namespaces, but it's fine to use them when
 * modifying an existing biome under that namespace. The reserved namespaces are
 * {@value #DEFAULT_NAMESPACE}, {@code brigadier}, and {@value #REALMS_NAMESPACE}.
 * {@value #DEFAULT_NAMESPACE} is also the default namespace used when no namespace is
 * provided.
 * 
 * <h3 id="path">Path</h3>
 * <p>The path of the identifier identifies the thing within the namespace, such as
 * between different items from the same mod. Additionally, this is sometimes used to
 * refer to a file path, such as in textures.
 * 
 * <h2 id="Creation">Creation</h2>
 * <p>There are many ways to create a new identifier:
 * 
 * <ul>
 * <li>{@link Identifier(String)} creates an identifier from a string in
 * {@code <namespace>:<path>} format. If the colon is missing, the created identifier
 * has the namespace {@value #DEFAULT_NAMESPACE} and the argument is used as the path.
 * When passed an invalid value, this throws {@link InvalidIdentifierException}.</li>
 * <li>{@link Identifier(String, String)} creates an identifier from namespace and path.
 * When passed an invalid value, this throws {@link InvalidIdentifierException}.</li>
 * <li>{@link #tryParse} creates an identifier from a string in
 * {@code <namespace>:<path>} format. If the colon is missing, the created identifier
 * has the namespace {@value #DEFAULT_NAMESPACE} and the argument is used as the path.
 * When passed an invalid value, this returns {@code null}.</li>
 * <li>{@link #of} creates an identifier from namespace and path.
 * When passed an invalid value, this returns {@code null}.</li>
 * <li>{@link #fromCommandInput} reads an identifier from command input reader.
 * When an invalid value is read, this throws {@link #COMMAND_EXCEPTION}.</li>
 * <li>{@link Identifier.Serializer} is a serializer for Gson.</li>
 * <li>{@link #CODEC} can be used to serialize and deserialize an identifier using
 * DataFixerUpper.</li>
 * </ul>
 * 
 * <h2 id="using">Using Identifier</h2>
 * <p>Identifiers identify several objects in the game. {@link
 * net.minecraft.registry.Registry} holds objects, such as blocks and items, that are
 * identified by an identifier. Textures are also identified using an identifier; such
 * an identifier is represented as a file path with an extension, such as {@code
 * minecraft:textures/entity/pig/pig.png}.
 * 
 * <p>The string representation of the identifier ({@code <namespace>:<path>}) can be
 * obtained by calling {@link #toString}. This always includes the namespace. An identifier
 * can be converted to a translation key using {@link #toTranslationKey(String)} method.
 * 
 * <h3 id="registrykey">RegistryKey</h3>
 * <p>Identifier is not type-aware; {@code minecraft:tnt} could refer to a TNT block, a TNT
 * item, or a TNT entity. To identify a registered object uniquely, {@link
 * net.minecraft.registry.RegistryKey} can be used. A registry key is a combination
 * of the registry's identifier and the object's identifier.
 */
public class Identifier {
    public static readonly char NamespaceSeparator = ':';
    public static readonly string DefaultNamespace = "mineclone";
    private readonly string _idNamespace;
    private readonly string _path;

    public Identifier(string namespaceIn, string path) {
        _idNamespace = namespaceIn;
        _path = path;
    }

    public static Identifier CreateDefault(string path) {
        return new Identifier(DefaultNamespace, path);
    }

    /**
     * {@return the path of the identifier}
     */
    public string GetPath() {
        return _path;
    }

    /**
     * {@return the namespace of the identifier}
     * 
     * <p>This returns {@value #DEFAULT_NAMESPACE} for identifiers created without a namespace.
     */
    public string GetNamespace() {
        return _idNamespace;
    }

    public Identifier WithPath(string path) {
        return new Identifier(_idNamespace, path);
    }

    public Identifier withPrefixedPath(string prefix) {
        return this.WithPath(prefix + _path);
    }

    public override string ToString() {
        return _idNamespace + ":" + _path;
    }

    public override bool Equals(Object o) {
        if (this == o) {
            return true;
        }
        if (o is Identifier identifier) {
            return _idNamespace.Equals(identifier._idNamespace) && _path.Equals(identifier._path);
        }
        return false;
    }

    public override int GetHashCode() {
        return 31 * _idNamespace.GetHashCode() + _path.GetHashCode();
    }

    public int CompareTo(Identifier identifier) {
        var to = string.Compare(_path, identifier._path, StringComparison.Ordinal);
        if (to == 0) {
            to = string.Compare(_idNamespace, identifier._idNamespace, StringComparison.Ordinal);
        }
        return to;
    }

    /**
     * {@return the string representation of the identifier with slashes and colons replaced
     * with underscores}
     */
    public string ToUnderscoreSeparatedString() {
        return ToString().Replace('/', '_').Replace(':', '_');
    }

    /**
     * {@return the long translation key, without omitting the default namespace}
     */
    public string ToTranslationKey() {
        return _idNamespace + "." + _path;
    }

    /**
     * {@return the short translation key, with the default namespace omitted if present}
     */
    public string ToShortTranslationKey() {
        return _idNamespace.Equals(DefaultNamespace) ? _path : ToTranslationKey();
    }

    /**
     * {@return the {@linkplain #toTranslationKey() long translation key} prefixed with
     * {@code prefix} and a dot}
     */
    public String toTranslationKey(String prefix) {
        return prefix + "." + this.ToTranslationKey();
    }

    private static bool IsCharValid(char c) {
        return c is >= '0' and <= '9' or >= 'a' and <= 'z' or '_' or ':' or '/' or '.' or '-';
    }

    /**
     * {@return whether {@code path} can be used as an identifier's path}
     */
    private static bool IsPathValid(string path) {
        return !path.Where((_, i) => !IsPathCharacterValid(path.ToCharArray()[i])).Any();
    }

    /**
     * {@return whether {@code namespace} can be used as an identifier's namespace}
     */
    private static bool IsNamespaceValid(string namespaceIn) {
        return !namespaceIn.Where((_, i) => !IsNamespaceCharacterValid(namespaceIn.ToCharArray()[i])).Any();
    }

    /**
     * {@return whether {@code character} is valid for use in identifier paths}
     */
    private static bool IsPathCharacterValid(char character) {
        return character is '_' or '-' or >= 'a' and <= 'z' or >= '0' and <= '9' or '/' or '.';
    }

    /**
     * {@return whether {@code character} is valid for use in identifier namespaces}
     */
    private static bool IsNamespaceCharacterValid(char character) {
        return character is '_' or '-' or >= 'a' and <= 'z' or >= '0' and <= '9' or '.';
    }

    /**
     * {@return whether {@code id} can be parsed as an identifier}
     */
    public static bool IsValid(string id) {
        var strings = id.Split(':');
        return IsNamespaceValid(string.IsNullOrEmpty(strings[0]) ? DefaultNamespace : strings[0]) && IsPathValid(strings[1]);
    }

    private static string ValidatePath(string namespaceIn, string path) {
        if (!IsPathValid(path)) {
            throw new InvalidIdentifierException("Non [a-z0-9/._-] character in path of location: " + namespaceIn + ":" + path);
        }
        return path;
    }

    public /* synthetic */ int compareTo(object other) {
        while (true) {
            other = (Identifier)other;
        }
    }

    private class InvalidIdentifierException : Exception {
        public InvalidIdentifierException(string message) {
            Debug.LogError(message);
        }
    }
}