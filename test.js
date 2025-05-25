function customReplacer(key, value) {
    if (value && typeof value === 'object' && value.constructor !== Object && value.constructor !== Array) {
      return {
        ...value,
        __type__: value.constructor.name,
      };
    }
    return value;
  }

  

class MyClass {
    constructor(x) {
        this.x = x;
    }

    print() {
        console.log("x:", this.x);
    }
}



const obj = {
    a: 1,
    b: new MyClass(42),
    c: null
};

const json = JSON.stringify(obj, customReplacer);
console.log("Serialized:", json);

const parsed = JSON.parse(json, customReviver);
parsed.b.print(); // Works as expected
