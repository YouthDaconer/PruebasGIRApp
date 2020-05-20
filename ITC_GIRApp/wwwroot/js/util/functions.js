/**
 * The identity function
 * @constant
 * @param { a } x The input value.
 * @template a
 * @returns { a } The same value.
 */
const identity = (x) => x;

/**
 *
 * @param { a } x
 * @template a
 * @returns { Val<a> }
 */
const val = (x) => Object.freeze(x);

/**
 * Ignore the passed value. This is often used to throw away results of a computation.
 * @param { any } _ The value to ignore.
 * @returns { void }
 */
const ignore = (_) => void 0;

/**
 *
 * @param { boolean } value
 * @returns { boolean }
 */
const not = (value) => !value

/**
 *
 * @param { Nullable<a> } value
 * @template a
 * @returns { value is null | undefined }
 */
const isNull = (value) => value == null || value == undefined;

/**
 *
 * @param { Nullable<string> | "" } value
 * @returns { value is null | undefined | "" }
 */
const isNullOrEmpty = (value) => isNull(value) || value === ""

/**
 *
 * @param { string | "" } value
 * @returns { value is "" }
 */
const isEmpty = (value) => value === ""

/**
 *
 * @param { a } x
 * @param { b } y
 * @template a, b
 * @returns { Pair<a, b> }
 */
const pair = (x, y) => val({ item1 : x, item2 : y })

/**
 *
 * @param { a } x
 * @param { b } y
 * @param { c } z
 * @template a, b, c
 * @returns { Triple<a, b, c> }
 */
const triple = (x, y, z) => val({ item1 : x, item2 : y, item3 : z })

/**
 *
 * @param { Pair<a, any> | Triple<a, any, any> } x
 * @template a
 * @returns { a }
 */
const fst = (x) => x.item1

/**
 *
 * @param { Pair<any, a> | Triple<any, a, any> } x
 * @template a
 * @returns { a }
 */
const snd = (x) => x.item2

/**
 *
 * @param { Triple<any, any, a> } x
 * @template a
 * @returns { a }
 */
const trd = (x) => x.item3

/**
 *
 * @param { Event } x
 * @returns { string }
 */
const getInputValue = (x) => x.target.value

/**
 *
 * @param { a } obj
 * @param { b } key
 * @template a
 * @template { keyof a } b
 */
const selectKey = (obj, key) => ({ [key] : obj[key] });

/**
 *
 * @type { Curry }
 */
const curry = (f) =>
{
    if (isNull(f)) throw new Error("Argument f cannot be null");

    if ( f.length == 0) return f;

    switch(f.length)
    {
        case 1 : return function(x)
            {
                return arguments.length == 0 ? f : f(x);
            }

        case 2 : return function(x, y)
            {
                return arguments.length == 2 ? f(x, y) : (z) => f(x, z);
            }

        case 3 : return function(x, y, z)
            {
                if ( arguments.length == 0 ) return f;

                switch(arguments.length)
                {
                    case 1 : return (arg1, arg2) => f(x, arg1, arg2) ;
                    case 2 : return (arg3) => (x, y, arg3);
                    case 3 : return f(x, y, z);
                }
            }

        case 4 : return function(x, y, z, w)
        {
            if ( arguments.length == 0 ) return f;

            switch(arguments.length)
                {
                    case 1 : return (arg1, arg2, arg3) => f(x, arg1, arg2, arg3) ;
                    case 2 : return (arg3, arg4) => (x, y, arg3, arg4);
                    case 3 : return (arg4) => f(x, y, z, arg4);
                    case 4 : return f(x, y, z, w);
                }
        }
    }
}

/**
 * @type { PipeLine }
 * @param { any } x
 * @param  {...Func<any,any>} fs
 */
const pipe = (x, ...fs) => fs.reduce((y, g) => g(y), x)

/**
 *
 * @param { string } json
 * @param { Func<Json, a> } complete
 * @param { Func<Error, a> } error
 * @template a
 * @returns { a }
 */
const tryParseJson = (json, success, error) =>
{
    try
    {
        return success(JSON.parse(json));
    }
    catch(e)
    {
        return error(e);
    }
}

/**
 *
 * @param { string } value
 * @returns { 0 | number }
 */
const toInt = (value) => isNaN(parseFloat(value)) ? 0 : parseFloat(value);

/**
 *
 * @param { a[] } xs
 * @param { a[] } ys
 * @template a
 * @returns { a[] }
 */
const concatList = (xs, ys) => [...xs, ...ys];

/**
 *
 * @param { a[] } xs
 * @param  {...a} ys
 * @template a
 */
const append = (xs, ...ys) => concatList(xs, ys);

const empty = {};

/**
 * @param { FormData } form
 * @returns { {} | Record<string, string> }
 */
const formDataToJson = (form) =>
    Array
        .from(form.entries())
        .reduce((json, entry) => ({ ...json, ... { [entry[0]] : entry[1] } }), empty);