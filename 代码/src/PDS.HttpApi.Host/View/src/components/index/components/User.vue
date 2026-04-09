<template>
  <div class="header-user">
    <router-link v-if="!user.isAuthenticated" to="/login" style="color: white !important">
      <a-icon type="user" />登录
    </router-link>
    <a-dropdown v-else :trigger="['click']">
      <a class="ant-dropdown-link" @click="(e) => e.preventDefault()">
        <a-icon type="user" />
        <span class="user-name">{{ user.name }}</span>
        <a-icon type="down" />
      </a>
      <a-menu slot="overlay">
        <a-menu-item class="font-size-14" v-if="user">
          <a href="javascript:void(0);" @click="reset()">
            <a-icon type="redo"></a-icon>&nbsp;重置密码
          </a>
        </a-menu-item>
        <a-menu-item class="font-size-14" v-if="user">
          <a href="javascript:void(0);" @click="logout()">
            <a-icon type="lock"></a-icon>&nbsp;退出登录
          </a>
        </a-menu-item>
      </a-menu>
    </a-dropdown>
    <reset-vue v-if="showForm" @close="close" />
  </div>
</template>
<script>
import { mapState } from "vuex";
import ResetVue from "./Reset.vue";
import { Logout } from "@/api/account";
import component from "@/lib/base";
export default {
  name: "User",
  mixins: [component],
  components: { ResetVue },
  data() {
    return {
      showForm: false,
    };
  },
  computed: {
    ...mapState("global", { user: "user" }),
  },
  methods: {
    reset: function () {
      this.showForm = true;
    },
    close: function () {
      this.showForm = false;
    },
    logout: async function () {
      if (!(await this.confirm("确定退出登录?"))) return;
      try {
        await Logout();
        window.location.reload();
      } catch (err) {
        this.$message.error(err.message);
      }
    },
  },
};
</script>
<style lang="less" scoped>
a {
  text-decoration: none !important;
}
.header-user{
  display: inline;
}
</style>
