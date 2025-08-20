# Excel API Abstraction Code Design

There are many libraries available to work with Excel file. However, each libary has its own speficic API that are quite different from each other. To avoid using vendor-specific API throughout the code-base, I have created a layer of abstraction for some Excel entities that are needed specifically in the code-base. 