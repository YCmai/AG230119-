<template>
  <a-modal :visible="true" :width="620" title="新增物料库存" class="default-hb-modal">
    <a-form-model
      :model="form"
      ref="form"
      class="ant-form-lable-100"
      layout="inline"
      :rules="rules"
    >
      <a-form-model-item label="SKU编码" prop="">
        <a-input v-model="form.sku" :disabled="isEdit" />
      </a-form-model-item>
      <a-form-model-item label="物料批次" prop="">
        <a-input v-model="form.batch" />
      </a-form-model-item>
      <a-form-model-item label="物料条码" prop="">
        <a-input v-model="form.barCode" />
      </a-form-model-item>
      <a-form-model-item label="储位编号">
        <a-input v-model="form.storageId"></a-input>
      </a-form-model-item>
      <a-form-model-item label="物料数量" >
        <a-input v-model="form.quatity"></a-input>
      </a-form-model-item>
      <a-form-model-item label="所在仓库">
        <a-input v-model="form.wareHouseid"></a-input>
      </a-form-model-item>
    </a-form-model>
    <template slot="footer">
      <a-button key="back" @click="close" size="small">
        {{ $t("actions.cancel") }}
      </a-button>
      <a-button key="submit" type="primary" :loading="loading" @click="save" size="small">
        {{ $t("actions.save") }}
      </a-button>
    </template>
  </a-modal>
</template>
<script>
import { InSearch, Create } from "@/api/stock/stock";
export default {
  name: "CreateInventoryForm",
  data() {
    return {
      loading: false,
      form: {
        sku: "",
        barCode: "",
        batch: "",
        storageId: "",
        quatity: "",
        wareHouseid: ""
      },
      rules: {
        sku: [{ required: true, message: "sku编码不能为空!", trigger: "blur" }],
        name: [{ required: true, message: "物料名称不能为空!", trigger: "blur" }],
      },
      isEdit: false,
    };
  },
  methods: {
    // load: async function (sku) {
    //   this.loading = true;
    //   try {
    //     let material = await InSearch(sku);
    //     this.form = Object.assign(this.form, material);
    //   } catch (err) {
    //     this.$message.error(err.message);
    //   } finally {
    //     this.loading = false;
    //   }
    // },
    save: async function () {
      if (!(await this.valid())) return;
      this.loading = true;
      try {
        await Create(this.form);
        this.$message.success("提交成功!");
        this.close();
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    close: function () {
      this.$emit("close");
    },
    valid: function () {
      return new Promise((resolve) => {
        this.$refs.form.validate((valid) => {
          resolve(valid);
        });
      });
    },
  },
};
</script>
