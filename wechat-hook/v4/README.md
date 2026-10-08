# SweetCheckout.Hook.WeChat

这个模块基于 [echotrace](https://github.com/ycccccccy/echotrace) 开发。

本模块仅适用于微信 Windows 版 `4.1.0.34`-`4.1.2.17`。

## 自行构建

执行构建脚本 `build.cmd` 进行构建，等待构建完成后，构建产物会被复制到 `Out` 目录。

在该目录新建一个配置文件 `config.properties`，内容如下。
```properties
# SweetCheckout Hook 配置文件
api_url=http://127.0.0.1:62233/api/hook/receive
hook_token=
wechat_key=
database_folder=auto

```

发布时，将 `Out` 目录下的所有文件，以及以下文件一同打包发布

+ `config.properties`
+ `LICENSE`
