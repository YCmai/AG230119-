<template>
  <a-layout class="layout">
    <a-layout-content class="content">
      <div class="login-container">
        <div class="intro">
          <h1>欢迎使用嘉腾WMS系统</h1>
          <p>
            嘉腾WMS系统是一个高效、可靠的仓库管理系统，帮助您优化仓库操作，提高工作效率。
          </p>
          <h1>Welcome to Jiateng WMS System</h1>
          <p>
            Jiateng WMS is an efficient and reliable warehouse management system that helps you optimize warehouse operations and improve productivity.
          </p>
        </div>
        <a-form @submit.prevent="handleSubmit" class="login-form">
          <a-form-item>
            <a-input v-model="user.userName" prefix-icon="user" placeholder="账号" />
          </a-form-item>
          <a-form-item>
            <a-input
            v-model="user.password"
              type="password"
              prefix-icon="lock"
              placeholder="密码"
            />
          </a-form-item>
          <a-form-item>
            <a-button type="primary" @click="login" html-type="submit" class="login-form-button">
              确认
            </a-button>
          </a-form-item>
        </a-form>
      </div>
    </a-layout-content>
    <a-layout-footer>联系电话: 400-830-1028</a-layout-footer>
  </a-layout>
</template>

<script>
import { Layout, Form, Input, Button, Icon } from 'ant-design-vue';

export default {
  name: 'LoginPage',
  components: {
    'a-layout': Layout,
    'a-layout-content': Layout.Content,
    'a-form': Form,
    'a-form-item': Form.Item,
    'a-input': Input,
    'a-button': Button,
    'a-icon': Icon,
  },
  data() {
    return {
      username: '',
      password: '',
    };
  },
  methods: {
    handleSubmit() {
      console.log('账号:', this.username);
      console.log('密码:', this.password);
      // 处理登录逻辑
    },
  },
};
</script>

<style>
.layout {
  min-height: 100vh;
}
.content {
  display: flex;
  justify-content: center;
  align-items: center;
  background: #f0f2f5;
}
.login-container {
  background: #fff;
  padding: 50px;
  border-radius: 10px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  text-align: center;
  max-width: 400px;
  width: 100%;
}
.intro {
  margin-bottom: 30px;
}
.intro h1 {
  margin-bottom: 10px;
}
.intro p {
  margin-bottom: 20px;
  color: #555;
}
.login-form {
  max-width: 300px;
  margin: 0 auto;
}
.login-form-button {
  width: 100%;
}
</style>
<script>
import "@/style/login.less";
import { Login } from "@/api/account";
import loadConfiguration from "@/lib/configuration";
export default {
  name: "Login",
  data() {
    return {
      icon: {},
      user: {
        userName: "",
        password: "",
      },
    };
  },
  methods: {
    login: async function () {
      let spin = this.$spin({ text: "正在登录..." });
      try {
        let result = await Login({
          userNameOrEmailAddress: this.user.userName,
          password: this.user.password,
        });
        if (result.result == 1) {
          await this.$message.success("登录成功!");
          await loadConfiguration();
          this.$router.push("/");
        } else {
          this.$message.error(result.description);
        }
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        spin.close();
      }
    },
    valid: function () {},
  },
};
</script>
