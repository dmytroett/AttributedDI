# AttributedDI
Register your services in IoC container in AOP style.

## TODO

### Features
- [x] Add module registration support
- [ ] Add decorator pattern support
- [x] Auto-generate interfaces from implementations

### Infrastructure
- [x] Switch to Source Generators
- [ ] Setup CI/CD pipeline
- [ ] Publish to NuGet
- [ ] Add diagnosers

### Documentation
- [ ] Create usage samples
- [ ] Write comprehensive wiki/documentation

### Tiny annoyances
- [ ] Classes with generated interfaces look like this: public partial MyClass : Namespace.IMyClass ... Namespace here is redundand. It is better to add using Namespace; statement instead.
- [ ] Exclude generated code from coverage. This applies to modules, generated partial classes and generated AddAttributeDi
