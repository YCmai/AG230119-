<template>
  <a-row>
    <a-col :span="2" class="container">
      <img src="@/assets/logo.png" class="logo" />
    </a-col>
    <a-col :span="16" style="text-align: left">
      <menus-vue></menus-vue>
    </a-col>
    <a-col :span="6" class="hb-layout-header-user" style="text-align: right">
      <user-vue></user-vue>
      <!-- <culture-vue></culture-vue> -->
    </a-col>
  </a-row>
</template>
<script>
import MenusVue from "./components/Menus.vue";
import UserVue from "./components/User.vue";
import CultureVue from "./components/Culture.vue";
// import microApp from "./header/MicroApp.vue";
// import user from "./header/User.vue";
// import Language from "./header/Language.vue";
export default {
  name: "Header",
  components: { MenusVue, UserVue, CultureVue },
  //components: { menus, user, Language },
};
</script>
<style scoped lang="less">
.container {
  padding: 0 5px;
  height: 38px;
  line-height: 30px;
}
.logo {
  width: 100%;
}
</style>
